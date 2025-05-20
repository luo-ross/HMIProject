using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RS.Widgets.Models
{
    public partial  class NotifyBase : ObservableObject, INotifyDataErrorInfo
    {
        public NotifyBase()
        {
            ErrorsDic = new Dictionary<string, IEnumerable<ValidErrorModel>>();
        }


        #region INotifyDataErrorInfo实现
        public  static readonly string DefaultErrorKey = "708819A8240246268C14CF142FF5B4A6";

        /// <summary>
        /// 错误字典Key 就是PropertyName 键值就是错误信息列表
        /// </summary>
        public Dictionary<string, IEnumerable<ValidErrorModel>> ErrorsDic;
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

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

        public IEnumerable GetErrors(string? propertyName = null)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                yield break;
            }
            if (ErrorsDic.ContainsKey(propertyName))
            {
                var errorList = ErrorsDic[propertyName];
                foreach (var error in errorList)
                {
                    yield return error.ErrorMsg;
                }
            }
            yield break;
        }

        /// <summary>
        /// 调用这个方法来验证某一单独的属性
        /// </summary>
        /// <param name="value"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 这是直接验证某一个类
        /// </summary>
        /// <returns></returns>
        public bool ValidObject()
        {
            ValidationContext validationContext = new ValidationContext(this);
            ICollection<ValidationResult>? validationResults = new List<ValidationResult>();
            var validResult = Validator.TryValidateObject(this, validationContext, validationResults, true);
            if (!validResult)
            {
                foreach (var validationResult in validationResults)
                {
                    if (validationResult.MemberNames.Count() == 0)
                    {
                        continue;
                    }
                    string propertyName = validationResult.MemberNames.First();
                    AddErrors(propertyName, new List<ValidationResult> { validationResult });
                }
            }
            return !HasErrors;
        }

        public void AddErrors(string? propertyName, ICollection<ValidationResult> validationResults, string validErrorKey = null)
        {
            string errorKey = GetErrorKey(validErrorKey);
            RemoveErrors(propertyName);

            //获取已有错误
            List<ValidErrorModel> validErrorModels = new List<ValidErrorModel>();
            if (ErrorsDic.ContainsKey(propertyName))
            {
                validErrorModels = ErrorsDic[propertyName].ToList();
            }
            //创建新错误
            var newValidErrors = validationResults.Select(t => new ValidErrorModel()
            {
                ErrorKey = errorKey,
                ErrorMsg = t.ErrorMessage
            });
            //合并新错误
            validErrorModels = validErrorModels.Concat(newValidErrors).ToList();
            //添加错误
            ErrorsDic.TryAdd(propertyName, validErrorModels);
            //触发错误通知
            OnErrorsChanged(propertyName);
        }

        public string GetErrorKey(string validErrorKey)
        {
            if (validErrorKey == null)
            {
                validErrorKey = DefaultErrorKey;
            }
            return validErrorKey;
        }

        public void RemoveErrors(string? propertyName, string validErrorKey = null)
        {
            string errorKey = GetErrorKey(validErrorKey);
            if (ErrorsDic.ContainsKey(propertyName))
            {
                var validErrorModels = ErrorsDic[propertyName].ToList();
                if (validErrorModels != null)
                {
                    validErrorModels.RemoveAll(t => t.ErrorKey == errorKey);
                }
                if (validErrorModels.Count == 0)
                {
                    ErrorsDic.Remove(propertyName);
                }
                OnErrorsChanged(propertyName);
            }
        }
        #endregion
    }
}
