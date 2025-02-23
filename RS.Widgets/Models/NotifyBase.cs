using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RS.Widgets.Models
{
    public class NotifyBase : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        /// <summary>
        /// 错误字典Key 就是PropertyName 键值就是错误信息列表
        /// </summary>
        public Dictionary<string, IEnumerable<string>> ErrorsDic;
        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
        public NotifyBase()
        {
            ErrorsDic = new Dictionary<string, IEnumerable<string>>();
        }

        public void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); ;
        }

        public bool OnPropertyChanged<T>(ref T field, T newValue, [CallerMemberName] string? propertyName = null)
        {
            if (!Equals(field, newValue))
            {
                field = newValue;
                OnPropertyChanged(propertyName);
                return true;
            }
            return false;
        }

        #region 数据验证处理

        public bool HasErrors
        {
            get
            {
                return ErrorsDic.Count > 0;
            }
        }

        public void OnErrorsChanged([CallerMemberName] string? propertyName = null)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName)); ;
        }

        public IEnumerable GetErrors(string? propertyName)
        {
            if (ErrorsDic.ContainsKey(propertyName))
            {
                var errorList = ErrorsDic[propertyName];
                foreach (var error in errorList)
                {
                    yield return error;
                }
            }
            yield break;
        }


        public bool ValidProperty(object? value, [CallerMemberName] string? propertyName = null)
        {
            ValidationContext validationContext = new ValidationContext(this)
            {
                MemberName = propertyName
            };
            ICollection<ValidationResult>? validationResults = new List<ValidationResult>();
            var validResult = Validator.TryValidateProperty(value, validationContext, validationResults);
            if (validResult)
            {
                //验证通过
                RemoveErrors(propertyName);
            }
            else
            {
                //验证失败
                AddErrors(propertyName, validationResults);
            }
            OnErrorsChanged(propertyName);

            return validResult;
        }

        public bool ValidObject()
        {
            ErrorsDic.Clear();
            ValidationContext validationContext = new ValidationContext(this);
            ICollection<ValidationResult>? validationResults = new List<ValidationResult>();
            var validResult = Validator.TryValidateObject(this, validationContext, validationResults, true);
            if (!validResult)
            {
                foreach (var validationResult in validationResults)
                {
                    string propertyName = validationResult.MemberNames.First();
                    AddErrors(propertyName,new List<ValidationResult> { validationResult });
                    OnErrorsChanged(propertyName);
                }
            }
            return !HasErrors;
        }

        private void AddErrors(string? propertyName, ICollection<ValidationResult> validationResults)
        {
            RemoveErrors(propertyName);
            ErrorsDic.TryAdd(propertyName, validationResults.Select(t => t.ErrorMessage));
        }

        private void RemoveErrors(string? propertyName)
        {
            if (ErrorsDic.ContainsKey(propertyName))
            {
                ErrorsDic.Remove(propertyName);
            }
        }

        #endregion
    }
}
