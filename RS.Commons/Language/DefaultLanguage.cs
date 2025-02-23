using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Commons.Language
{
    /// <summary>
    /// 系统的语言基类，默认也即是中文版本
    /// </summary>
    public class DefaultLanguage
    {
        public virtual string AuthorizationFailed => "系统授权失败，需要使用激活码授权，谢谢支持。";
        public virtual string ConnectedFailed => "连接失败：";
        public virtual string ConnectedSuccess => "连接成功！";
        public virtual string UnknownError => "未知错误";
        public virtual string ErrorCode => "错误代号";
        public virtual string TextDescription => "文本描述";
        public virtual string ExceptionMessage => "错误信息：";
        public virtual string ExceptionSourse => "错误源：";
        public virtual string ExceptionType => "错误类型：";
        public virtual string ExceptionStackTrace => "错误堆栈：";
        public virtual string ExceptopnTargetSite => "错误方法：";
        public virtual string ExceprionCustomer => "用户自定义方法出错：";
        public virtual string SuccessText => "成功";
        public virtual string ErrorText => "错误";
        public virtual string WarningText => "警告";
        public virtual string InfoText => "提示";
        public virtual string TwoParametersLengthIsNotSame => "两个参数的个数不一致";
        public virtual string NotSupportedDataType => "输入的类型不支持，请重新输入";
        public virtual string NotSupportedFunction => "当前的功能逻辑不支持";
        public virtual string DataLengthIsNotEnough => "接收的数据长度不足，应该值:{0},实际值:{1}";
        public virtual string ReceiveDataTimeout => "接收数据超时：";
        public virtual string ReceiveDataLengthTooShort => "接收的数据长度太短：";
        public virtual string MessageTip => "消息提示：";
        public virtual string Close => "关闭";
        public virtual string Time => "时间：";
        public virtual string SoftWare => "软件：";
        public virtual string BugSubmit => "Bug提交";
        public virtual string MailServerCenter => "邮件发送系统";
        public virtual string MailSendTail => "邮件服务系统自动发出，请勿回复！";
        public virtual string IpAddresError => "Ip地址输入异常，格式不正确";
        public virtual string Send => "发送";
        public virtual string Receive => "接收";

        /***********************************************************************************
         * 
         *    系统相关的错误信息
         * 
         ************************************************************************************/

        public virtual string SystemInstallOperater => "安装新系统：IP为";
        public virtual string SystemUpdateOperater => "更新新系统：IP为";


        /***********************************************************************************
         * 
         *    套接字相关的信息描述
         * 
         ************************************************************************************/

        public virtual string SocketIOException => "套接字传送数据异常：";
        public virtual string SocketSendException => "同步数据发送异常：";
        public virtual string SocketHeadReceiveException => "指令头接收异常：";
        public virtual string SocketContentReceiveException => "内容数据接收异常：";
        public virtual string SocketContentRemoteReceiveException => "对方内容数据接收异常：";
        public virtual string SocketAcceptCallbackException => "异步接受传入的连接尝试";
        public virtual string SocketReAcceptCallbackException => "重新异步接受传入的连接尝试";
        public virtual string SocketSendAsyncException => "异步数据发送出错:";
        public virtual string SocketEndSendException => "异步数据结束挂起发送出错";
        public virtual string SocketReceiveException => "异步数据发送出错:";
        public virtual string SocketEndReceiveException => "异步数据结束接收指令头出错";
        public virtual string SocketRemoteCloseException => "远程主机强迫关闭了一个现有的连接";


        /***********************************************************************************
         * 
         *    文件相关的信息
         * 
         ************************************************************************************/


        public virtual string FileDownloadSuccess => "文件下载成功";
        public virtual string FileDownloadFailed => "文件下载异常";
        public virtual string FileUploadFailed => "文件上传异常";
        public virtual string FileUploadSuccess => "文件上传成功";
        public virtual string FileDeleteFailed => "文件删除异常";
        public virtual string FileDeleteSuccess => "文件删除成功";
        public virtual string FileReceiveFailed => "确认文件接收异常";
        public virtual string FileNotExist => "文件不存在";
        public virtual string FileSaveFailed => "文件存储失败";
        public virtual string FileLoadFailed => "文件加载失败";
        public virtual string FileSendClientFailed => "文件发送的时候发生了异常";
        public virtual string FileWriteToNetFailed => "文件写入网络异常";
        public virtual string FileReadFromNetFailed => "从网络读取文件异常";
        public virtual string FilePathCreateFailed => "文件夹路径创建失败：";
        public virtual string FileRemoteNotExist => "对方文件不存在，无法接收！";

        /***********************************************************************************
         * 
         *    服务器的引擎相关数据
         * 
         ************************************************************************************/

        public virtual string TokenCheckFailed => "接收验证令牌不一致";
        public virtual string TokenCheckTimeout => "接收验证超时:";
        public virtual string CommandHeadCodeCheckFailed => "命令头校验失败";
        public virtual string CommandLengthCheckFailed => "命令长度检查失败";
        public virtual string NetClientAliasFailed => "客户端的别名接收失败：";
        public virtual string NetClientAccountTimeout => "等待账户验证超时：";
        public virtual string NetEngineStart => "启动引擎";
        public virtual string NetEngineClose => "关闭引擎";
        public virtual string NetClientOnline => "上线";
        public virtual string NetClientOffline => "下线";
        public virtual string NetClientBreak => "异常掉线";
        public virtual string NetClientFull => "服务器承载上限，收到超出的请求连接。";
        public virtual string NetClientLoginFailed => "客户端登录中错误：";
        public virtual string NetHeartCheckFailed => "心跳验证异常：";
        public virtual string NetHeartCheckTimeout => "心跳验证超时，强制下线：";
        public virtual string DataSourseFormatError => "数据源格式不正确";
        public virtual string ServerFileCheckFailed => "服务器确认文件失败，请重新上传";
        public virtual string ClientOnlineInfo => "客户端 [ {0} ] 上线";
        public virtual string ClientOfflineInfo => "客户端 [ {0} ] 下线";
        public virtual string ClientDisableLogin => "客户端 [ {0} ] 不被信任，禁止登录";

        /***********************************************************************************
         * 
         *    Client 相关
         * 
         ************************************************************************************/

        public virtual string ReConnectServerSuccess => "重连服务器成功";
        public virtual string ReConnectServerAfterTenSeconds => "在10秒后重新连接服务器";
        public virtual string KeyIsNotAllowedNull => "关键字不允许为空";
        public virtual string KeyIsExistAlready => "当前的关键字已经存在";
        public virtual string KeyIsNotExist => "当前订阅的关键字不存在";
        public virtual string ConnectingServer => "正在连接服务器...";
        public virtual string ConnectFailedAndWait => "连接断开，等待{0}秒后重新连接";
        public virtual string AttemptConnectServer => "正在尝试第{0}次连接服务器";
        public virtual string ConnectServerSuccess => "连接服务器成功";
        public virtual string GetClientIpaddressFailed => "客户端IP地址获取失败";
        public virtual string ConnectionIsNotAvailable => "当前的连接不可用";
        public virtual string DeviceCurrentIsLoginRepeat => "当前设备的id重复登录";
        public virtual string DeviceCurrentIsLoginForbidden => "当前设备的id禁止登录";
        public virtual string PasswordCheckFailed => "密码验证失败";
        public virtual string DataTransformError => "数据转换失败，源数据：";
        public virtual string RemoteClosedConnection => "远程关闭了连接";

        /***********************************************************************************
         * 
         *    日志 相关
         * 
         ************************************************************************************/
        public virtual string LogNetDebug => "调试";
        public virtual string LogNetInfo => "信息";
        public virtual string LogNetWarn => "警告";
        public virtual string LogNetError => "错误";
        public virtual string LogNetFatal => "致命";
        public virtual string LogNetAbandon => "放弃";
        public virtual string LogNetAll => "全部";

    }
}
