using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using FluentValidation;

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
        private AbstractValidator<TImport> Validator { get; set; }

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
            return configurationKeys.All(key => header.Keys.Contains(key)) && 
                   configurationKeys.Count == header.Keys.Count;
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

            return result;
        }

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
                if (propertyCellValues.ContainsKey(error.PropertyName))
                {
                    AddDictionaryListItem(result, propertyCellValues[error.PropertyName], error.ErrorMessage);
                }
                else if (propertyCellComments.ContainsKey(error.PropertyName))
                {
                    AddDictionaryListItem(result, propertyCellComments[error.PropertyName], error.ErrorMessage);
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

            throw new NotImplementedException();
        }

        #endregion
    }
}