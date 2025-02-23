namespace RS.WPFApp.Enums
{
    /// <summary>
    /// 注册任务状态
    /// </summary>
    public enum RegisterTaskStatus
    {
        /// <summary>
        /// 获取注册邮箱验证码
        /// </summary>
        GetEmailVerification,

        /// <summary>
        /// 注册邮箱验证码验证
        /// </summary>
        EmailVerificationValid,

        /// <summary>
        /// 获取注册短信验证码
        /// </summary>
        GetSMSVerification,

        /// <summary>
        /// 注册短信验证码验证
        /// </summary>
        SMSVerificationValid,
    }
}
