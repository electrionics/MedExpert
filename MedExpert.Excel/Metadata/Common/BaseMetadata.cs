using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using FluentValidation;
using MedExpert.Domain.Enums;
using MedExpert.Excel.Converters.Common;
using MedExpert.Excel.Converters.Common.Base;
using MedExpert.Excel.Converters.Common.Default;
using MedExpert.Excel.Model;
using MedExpert.Excel.Model.Common;
using MedExpert.Excel.Model.Symptoms;

// ReSharper disable StringLiteralTypo

namespace MedExpert.Excel.Metadata.Common
{
    public abstract class BaseMetadata<TImport>
        where TImport: new()
    {
        protected BaseMetadata()
        {
            CellValues = new Dictionary<string, PropertyInfo>();
            CellComments = new Dictionary<string, PropertyInfo>();
            CellValueHeaders = new Dictionary<string, PropertyInfo>();
            CellCommentHeaders = new Dictionary<string, PropertyInfo>();
            PropertyToColumnValueLastSet = new Dictionary<TImport, Dictionary<string, Tuple<int, string>>>();
            PropertyToColumnCommentLastSet = new Dictionary<TImport, Dictionary<string, Tuple<int, string>>>();
            Converters = new Dictionary<Type, IConverter>();
            foreach (var defaultConverter in _defaultConverters)
            {
                foreach (var type in defaultConverter.GetTypes())
                {
                    Converters[type] = defaultConverter;
                }
            }
        }

        private readonly List<IConverter> _defaultConverters = new()
        {
            new BoolConverter(), new StringConverter(), new IntConverter(), new DecimalConverter(), new DateTimeConverter("dd.MM.yyyy")
        };
        
        private Dictionary<Type, IConverter> Converters { get; }
        
        private Dictionary<TImport, Dictionary<string, Tuple<int, string>>> PropertyToColumnValueLastSet { get; }
        private Dictionary<TImport, Dictionary<string, Tuple<int, string>>> PropertyToColumnCommentLastSet { get; }

        private Dictionary<string, PropertyInfo> CellValues { get; }
        private Dictionary<string, PropertyInfo> CellComments { get; }
        private Dictionary<string, PropertyInfo> CellValueHeaders { get; }
        private Dictionary<string, PropertyInfo> CellCommentHeaders { get; }
        private PropertyInfo CellDictionary { get; set; }
        private AbstractValidator<TImport> Validator { get; set; }
        private AbstractValidator<List<string>> DynamicHeaderValidator { get; set; }

        #region Build Metadata
        
        protected void CellValue<TProperty>(Expression<Func<TImport, TProperty>> propertyExpression, string columnName, Expression<Func<TImport, string>> headerPropertyExpression = null)
        {
            CellItem(propertyExpression, columnName, true);
            if (headerPropertyExpression != null)
            {
                CellHeader(headerPropertyExpression, columnName, true);
            }
        }

        protected void CellComment<TProperty>(Expression<Func<TImport, TProperty>> propertyExpression, string columnName, Expression<Func<TImport, string>> headerPropertyExpression = null)
        {
            CellItem(propertyExpression, columnName, false);
            if (headerPropertyExpression != null)
            {
                CellHeader(headerPropertyExpression, columnName, false);
            }
        }

        protected void CellsDictionary<TProperty>(
            Expression<Func<TImport, Dictionary<string, TProperty>>> dictionaryExpression)
        {
            if (dictionaryExpression.Body is MemberExpression memberSelectorExpression)
            {
                var property = memberSelectorExpression.Member as PropertyInfo;
                if (property != null)
                {
                    CellDictionary = property;
                }
            }
        }

        protected void AddConverters(IEnumerable<IConverter> converters)
        {
            foreach (var converter in converters)
            {
                foreach (var type in converter.GetTypes())
                {
                    Converters[type] = converter;
                }
            }
        }

        protected void HeaderValidator(AbstractValidator<List<string>> validator)
        {
            DynamicHeaderValidator = validator;
        }

        protected void EntityValidator(AbstractValidator<TImport> validator)
        {
            Validator = validator;
        }

        private void CellHeader(Expression<Func<TImport, string>> headerPropertyExpression, string columnName, bool isValue)
        {
            if (headerPropertyExpression.Body is MemberExpression memberSelectorExpression)
            {
                var property = memberSelectorExpression.Member as PropertyInfo;
                if (property != null)
                {
                    if (isValue)
                    {
                        CellValueHeaders.Add(columnName, property);
                    }
                    else
                    {
                        CellCommentHeaders.Add(columnName, property);
                    }
                }
                else
                {
                    throw new ArgumentException("Not property of instance", nameof(headerPropertyExpression));
                }
            }
            else
            {
                throw new ArgumentException("Not property of instance", nameof(headerPropertyExpression));
            }
        }
        
        private void CellItem<TProperty>(Expression<Func<TImport, TProperty>> propertyExpression, string columnName, bool isValue)
        {
            if (propertyExpression.Body is MemberExpression memberSelectorExpression)
            {
                var property = memberSelectorExpression.Member as PropertyInfo;
                if (property != null)
                {
                    if (isValue)
                    {
                        CellValues.Add(columnName, property);
                    }
                    else
                    {
                        CellComments.Add(columnName, property);
                    }
                }
                else
                {
                    throw new ArgumentException("Not property of instance", nameof(propertyExpression));
                }
            }
            else
            {
                throw new ArgumentException("Not property of instance", nameof(propertyExpression));
            }
        }
        
        #endregion

        #region Validate Header

        public List<string> ValidateHeader(List<KeyValuePair<string, string>> headerItems)
        {
            var configurationKeys = CellValues.Keys.Union(CellComments.Keys).ToList();
            var headerKeys = headerItems.Select(pair => pair.Key).ToList();
            var dynamicHeaderKeys = headerKeys.Where(key => !configurationKeys.Contains(key)).ToList();

            var result = new List<string>();
            if (!configurationKeys.All(key => headerKeys.Contains(key)))
            {
                result.Add("Не все обязательные столбцы содержатся в строке-заголовке файла: " +
                           $"{string.Join(", ", configurationKeys.Where(key => headerKeys.Contains(key)).ToList())}.");
            }

            if (DynamicHeaderValidator == null && !headerKeys.All(key => configurationKeys.Contains(key)))
            {
                result.Add("Неоторые столбцы являются лишними в строке-заголовке файла: " +
                           $"{string.Join(", ", headerKeys.Where(key => configurationKeys.Contains(key)).ToList())}.");
            }

            if (DynamicHeaderValidator != null)
            {
                var validationResult = DynamicHeaderValidator.Validate(dynamicHeaderKeys);
                result.AddRange(validationResult.Errors.Select(error => error.ErrorMessage));
            }

            return result;
        }

        #endregion

        #region Validate Cells
        
        public Dictionary<string, string> ValidateCells(Dictionary<string, string> header, IReadOnlyDictionary<string, Tuple<string, string>> columnCells)
        {
            var result = new Dictionary<string, string>();
            
            foreach (var (key, property) in CellValues)
            {
                result[header[key]] = ValidateStringValue(property.PropertyType, columnCells.GetValueOrDefault(header[key]) ?? Tuple.Create("",""));
            }
            
            foreach (var (key, property) in CellComments)
            {
                result[header[key]] = ValidateStringComment(property.PropertyType, columnCells.GetValueOrDefault(header[key]) ?? Tuple.Create("",""));
            }

            if (CellDictionary == null) return result;
            
            var configurationKeys = CellValues.Keys.Union(CellComments.Keys);
            foreach (var (key, column) in header.Where(x => !configurationKeys.Contains(x.Key)))
            {
                result[header[key]] = ValidateStringValue(CellDictionary.PropertyType.GenericTypeArguments[1],
                    columnCells.GetValueOrDefault(header[key]) ?? Tuple.Create("", ""));
            }

            return result;
        }

        private string ValidateString(Type propertyType, string value)
        {
            if (Converters.TryGetValue(propertyType, out var converter))
            {
                return converter.ValidateString(propertyType, value);
            }

            throw new ArgumentException("Validation not implemented", nameof(propertyType));
        }

        private string ValidateStringValue(Type propertyType, Tuple<string, string> cell)
        {
            return ValidateString(propertyType, cell.Item1);
        }
        
        private string ValidateStringComment(Type propertyType, Tuple<string, string> cell)
        {
            return ValidateString(propertyType, cell.Item2);
        }

        #endregion
        
        #region Validate Entity

        public Dictionary<string, List<string>> ValidateEntity(Dictionary<string, string> header, TImport entity)
        {
            var validationResult = Validator.Validate(entity);

            var result = new Dictionary<string, List<string>>();
            foreach (var error in validationResult.Errors)
            {
                var cellErrorPropertyName = error.PropertyName.Split('.')[0];
                var dictionaryErrorPropertyName = error.FormattedMessagePlaceholderValues["PropertyName"]?.ToString();

                if (PropertyToColumnValueLastSet.ContainsKey(entity) && PropertyToColumnValueLastSet[entity].ContainsKey(cellErrorPropertyName))
                {
                    AddDictionaryListItem(result, header[PropertyToColumnValueLastSet[entity][cellErrorPropertyName].Item2], error.ErrorMessage);
                }
                else if (PropertyToColumnCommentLastSet.ContainsKey(entity) && PropertyToColumnCommentLastSet[entity].ContainsKey(cellErrorPropertyName))
                {
                    AddDictionaryListItem(result, header[PropertyToColumnCommentLastSet[entity][cellErrorPropertyName].Item2], error.ErrorMessage);
                }
                else if (CellDictionary != null && header.ContainsKey(dictionaryErrorPropertyName!))
                {
                    AddDictionaryListItem(result, header[dictionaryErrorPropertyName], error.ErrorMessage);
                }
                else
                {
                    AddDictionaryListItem(result, string.Empty, error.ErrorMessage);
                }
            }

            if (PropertyToColumnValueLastSet.TryGetValue(entity, out var propertyToColumnValuePair))
            {
                foreach (var propertyToColumnPair in propertyToColumnValuePair.Where(pair => pair.Value.Item1 > 1))
                {
                    AddDictionaryListItem(result,
                        header[PropertyToColumnValueLastSet[entity][propertyToColumnPair.Key].Item2],
                        $"Значение ячейки для столбца с заголовком '{propertyToColumnPair.Value.Item2}' присутствует также и в других столбцах.");
                }
            }

            if (PropertyToColumnCommentLastSet.TryGetValue(entity, out var propertyToColumnCommentPair))
            {
                foreach (var propertyToColumnPair in propertyToColumnCommentPair.Where(pair => pair.Value.Item1 > 1))
                {
                    AddDictionaryListItem(result, header[PropertyToColumnCommentLastSet[entity][propertyToColumnPair.Key].Item2], 
                        $"Комментарий к ячейке для столбца с заголовком '{propertyToColumnPair.Value.Item2}' присутствует также и в других столбцах.");
                }
            }

            return result;
        }

        private static void AddDictionaryListItem(IDictionary<string, List<string>> result, string key, string value)
        {
            if (!result.ContainsKey(key))
            {
                result[key] = new List<string>();
            }
            
            result[key].Add(value);
        }

        private static void AddDictionaryDictItem(IDictionary<TImport, Dictionary<string, Tuple<int, string>>> result, TImport key1,
            string key2, string value)
        {
            if (!result.ContainsKey(key1))
            {
                result[key1] = new Dictionary<string, Tuple<int, string>>();
            }

            var setCount = 1;
            if (result[key1].TryGetValue(key2, out var tuple))
            {
                setCount = tuple.Item1 + 1;
            }
            
            result[key1][key2] = Tuple.Create(setCount, value);
        }

        #endregion

        #region Create Entity
        
        public TImport CreateEntity(Dictionary<string, string> header, IReadOnlyDictionary<string, Tuple<string, string>> columnCells)
        {
            var entity = new TImport();
            
            foreach (var key in CellValues.Keys)
            {
                SetValue(key, entity, columnCells[header[key]]);
            }

            foreach (var key in CellComments.Keys)
            {
                SetComment(key, entity, columnCells[header[key]]);
            }

            if (CellDictionary != null)
            {
                var configurationKeys = CellValues.Keys.Union(CellComments.Keys).ToList();
                var dynamicHeaderKeys = header.Keys.Where(key => !configurationKeys.Contains(key)).ToList();

                foreach (var key in dynamicHeaderKeys)
                {
                    SetDictionaryItem(key, entity, columnCells[header[key]]);
                }
            }

            return entity;
        }

        private void SetValue(string columnName, TImport entity, Tuple<string, string> cell)
        {
            var property = CellValues[columnName];
            var samePropertyCount = CellValues.Count(x => x.Value.Name == property.Name);
            
            var valueObj = ConvertStringValue(property.PropertyType, cell.Item1);

            if (samePropertyCount > 1 && valueObj == null) return;
            
            property.SetValue(entity, valueObj, null);
            
            SetValueHeader(columnName, entity);

            AddDictionaryDictItem(PropertyToColumnValueLastSet, entity, property.Name, columnName);
        }
        
        private void SetComment(string columnName, TImport entity, Tuple<string, string> cell)
        {
            var property = CellComments[columnName];
            var samePropertyCount = CellComments.Count(x => x.Value.Name == property.Name);
            
            var valueObj = ConvertStringValue(property.PropertyType, cell.Item2);

            if (samePropertyCount > 1 && valueObj == null) return;
            
            property.SetValue(entity, valueObj, null);
            
            SetCommentHeader(columnName, entity);

            AddDictionaryDictItem(PropertyToColumnCommentLastSet, entity, property.Name, columnName);
        }

        private void SetValueHeader(string columnName, TImport entity)
        {
            if (CellValueHeaders.ContainsKey(columnName))
            {
                var property = CellValueHeaders[columnName];
            
                property.SetValue(entity, columnName, null);
            }
        }
        
        private void SetCommentHeader(string columnName, TImport entity)
        {
            if (CellCommentHeaders.ContainsKey(columnName))
            {
                var property = CellCommentHeaders[columnName];
            
                property.SetValue(entity, columnName, null);
            }
        }

        private void SetDictionaryItem(string key, TImport entity, Tuple<string, string> cell)
        {
            var property = CellDictionary;

            var valueObj = ConvertStringValue(property.PropertyType.GenericTypeArguments[1], cell.Item1, cell.Item2);

            var indexerPropertyInfo = property.PropertyType.GetProperty("Item");

            var dictionary = property.GetValue(entity);

            indexerPropertyInfo.SetValue(dictionary, valueObj, new[] { key });
        }

        private object ConvertStringValue(Type propertyType, string value, string additionalValue = null)
        {
            if (Converters.TryGetValue(propertyType, out var converter))
            {
                return converter.ConvertStringValue(propertyType, value, additionalValue);
            }

            throw new ArgumentException("Conversion not implemented", nameof(propertyType));
        }

        #endregion
    }
}