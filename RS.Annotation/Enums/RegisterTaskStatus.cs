namespace RS.Annotation.Enums
{
    /// <summary>
    /// 注册任务状态
    /// </summary>
    public enum RegisterTaskStatus
    {
        /// <summary>
        /// 获取注册邮箱验证码
        /// </summary>
        GetEmailVerify,

        /// <summary>
        /// 注册邮箱验证码验证
        /// </summary>
        EmailVerifyValid,

        /// <summary>
        /// 获取注册短信验证码
        /// </summary>
        GetSMSVerify,

        /// <summary>
        /// 注册短信验证码验证
        /// </summary>
        SMSVerifyValid,
    }
}
