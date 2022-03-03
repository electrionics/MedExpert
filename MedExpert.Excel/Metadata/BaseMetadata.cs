using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Results;
using MedExpert.Domain.Enums;
using MedExpert.Excel.Model;

// ReSharper disable StringLiteralTypo

namespace MedExpert.Excel.Metadata
{
    public abstract class BaseMetadata<TImport>
        where TImport: new()
    {
        protected BaseMetadata()
        {
            CellValues = new Dictionary<string, PropertyInfo>();
            CellComments = new Dictionary<string, PropertyInfo>();
        }

        private Dictionary<string, PropertyInfo> CellValues { get; }
        private Dictionary<string, PropertyInfo> CellComments { get; }
        private PropertyInfo CellDictionary { get; set; }
        private AbstractValidator<TImport> Validator { get; set; }
        private AbstractValidator<List<string>> DynamicHeaderValidator { get; set; }

        private Dictionary<string, string> PropertyCellValues => CellValues.ToDictionary(
            x => x.Value.Name,
            x => x.Key);
        private Dictionary<string, string> PropertyCellComments => CellComments.ToDictionary(
            x => x.Value.Name,
            x => x.Key);

        #region Build Metadata
        
        protected void CellValue<TProperty>(Expression<Func<TImport, TProperty>> propertyExpression, string columnName)
        {
            CellItem(propertyExpression, columnName, true);
        }

        protected void CellComment<TProperty>(Expression<Func<TImport, TProperty>> propertyExpression, string columnName)
        {
            CellItem(propertyExpression, columnName, false);
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

        protected void HeaderValidator(AbstractValidator<List<string>> validator)
        {
            DynamicHeaderValidator = validator;
        }

        protected void EntityValidator(AbstractValidator<TImport> validator)
        {
            Validator = validator;
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

        public bool ValidateHeader(Dictionary<string, string> header)
        {
            var configurationKeys = CellValues.Keys.Union(CellComments.Keys).ToList();
            var dynamicHeaderKeys = header.Keys.Where(key => !configurationKeys.Contains(key)).ToList();
            return configurationKeys.All(key => header.Keys.Contains(key)) && 
                   (configurationKeys.Count == header.Keys.Count || 
                    DynamicHeaderValidator != null && 
                    DynamicHeaderValidator.Validate(dynamicHeaderKeys).IsValid);
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

            if (CellDictionary != null)
            {
                var configurationKeys = CellValues.Keys.Union(CellComments.Keys);
                foreach (var (key, column) in header.Where(x => !configurationKeys.Contains(x.Key)))
                {
                    result[header[key]] = ValidateStringValue(CellDictionary.PropertyType.GenericTypeArguments[1],
                        columnCells.GetValueOrDefault(header[key]) ?? Tuple.Create("", ""));
                }
            }

            return result;
        }

        private static readonly Regex IntervalRegex = new("(\\d+(,?\\d+)?)-(\\d+(,?\\d+)?)");

        private static string ValidateString(Type propertyType, string value)
        {
            if (propertyType == typeof(bool) || propertyType == typeof(bool?))
            {
                return value is "0" or "1" or "" ? null : "Значение должно быть равно '1', '0' или быть пустым.";
            }

            if (propertyType == typeof(string))
            {
                return null;
            }

            if (propertyType == typeof(Sex))
            {
                return value == "М" || value == "Ж"
                    ? null
                    : "Значение должно быть равно 'М' или 'Ж'.";
            }

            if (propertyType == typeof(IntervalModel))
            {
                return string.IsNullOrEmpty(value)
                    ? "Значение не должно быть пустым"
                    : !IntervalRegex.IsMatch(value)
                        ? "Некорректный формат интервала (пример: '0,1-0,3')"
                        : null;
            }

            throw new NotImplementedException();
        }

        private static string ValidateStringValue(Type propertyType, Tuple<string, string> cell)
        {
            return ValidateString(propertyType, cell.Item1);
        }
        
        private static string ValidateStringComment(Type propertyType, Tuple<string, string> cell)
        {
            return ValidateString(propertyType, cell.Item2);
        }

        #endregion
        
        #region Validate Entity

        public Dictionary<string, List<string>> ValidateEntity(Dictionary<string, string> header, TImport entity)
        {
            var validationResult = Validator.Validate(entity);
            
            var propertyCellValues = PropertyCellValues;
            var propertyCellComments = PropertyCellComments;

            var result = new Dictionary<string, List<string>>();
            foreach (var error in validationResult.Errors)
            {
                var cellErrorPropertyName = error.PropertyName.Split('.')[0];
                var dictionaryErrorPropertyName = error.FormattedMessagePlaceholderValues["PropertyName"]?.ToString();
                
                if (propertyCellValues.ContainsKey(cellErrorPropertyName))
                {
                    AddDictionaryListItem(result, header[propertyCellValues[cellErrorPropertyName]], error.ErrorMessage);
                }
                else if (propertyCellComments.ContainsKey(cellErrorPropertyName))
                {
                    AddDictionaryListItem(result, header[propertyCellComments[cellErrorPropertyName]], error.ErrorMessage);
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
            
            var valueObj = ConvertStringValue(property.PropertyType, cell.Item1);
            
            property.SetValue(entity, valueObj, null);
        }
        
        private void SetComment(string columnName, TImport entity, Tuple<string, string> cell)
        {
            var property = CellComments[columnName];
            
            var valueObj = ConvertStringValue(property.PropertyType, cell.Item2);
            
            property.SetValue(entity, valueObj, null);
        }

        private void SetDictionaryItem(string key, TImport entity, Tuple<string, string> cell)
        {
            var property = CellDictionary;

            var valueObj = ConvertStringValue(property.PropertyType.GenericTypeArguments[1], cell.Item1);

            var indexerPropertyInfo = property.PropertyType.GetProperty("Item");

            var dictionary = property.GetValue(entity);

            indexerPropertyInfo.SetValue(dictionary, valueObj, new[] { key });
        }

        private static object ConvertStringValue(Type propertyType, string value)
        {
            if (propertyType == typeof(bool))
            {
                return value == "1";
            }

            if (propertyType == typeof(string))
            {
                return value;
            }
            
            if (propertyType == typeof(Sex))
            {
                return value == "М" ? Sex.Male : Sex.Female;
            }

            if (propertyType == typeof(IntervalModel))
            {
                var groups = IntervalRegex.Match(value).Groups;
                return new IntervalModel
                {
                    ValueMin = decimal.Parse(groups[1].Value.Replace(",", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator)),
                    ValueMax = decimal.Parse(groups[3].Value.Replace(",", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator))
                };
            }

            throw new NotImplementedException();
        }

        #endregion
    }
}