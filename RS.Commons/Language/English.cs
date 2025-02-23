using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Commons.Language
{
    /// <summary>
    /// English Version Text
    /// </summary>
    public class English : DefaultLanguage
    {
        /***********************************************************************************
        * 
        *    Normal Info
        * 
        ************************************************************************************/

        public override string AuthorizationFailed => "System authorization failed, need to use activation code authorization, thank you for your support.";
        public override string ConnectedFailed => "Connected Failed: ";
        public override string ConnectedSuccess => "Connect Success !";
        public override string UnknownError => "Unknown Error";
        public override string ErrorCode => "Error Code: ";
        public override string TextDescription => "Description: ";
        public override string ExceptionMessage => "Exception Info: ";
        public override string ExceptionSourse => "Exception Sourse：";
        public override string ExceptionType => "Exception Type：";
        public override string ExceptionStackTrace => "Exception Stack: ";
        public override string ExceptopnTargetSite => "Exception Method: ";
        public override string ExceprionCustomer => "Error in user-defined method: ";
        public override string SuccessText => "Success";
        public override string ErrorText => "Error";
        public override string WarningText => "Warning";
        public override string InfoText => "Info";
        public override string TwoParametersLengthIsNotSame => "Two Parameter Length is not same";
        public override string NotSupportedDataType => "Unsupported DataType, input again";
        public override string NotSupportedFunction => "The current feature logic does not support";
        public override string DataLengthIsNotEnough => "Receive length is not enough，Should:{0},Actual:{1}";
        public override string ReceiveDataTimeout => "Receive timeout: ";
        public override string ReceiveDataLengthTooShort => "Receive length is too short: ";
        public override string MessageTip => "Message prompt:";
        public override string Close => "Close";
        public override string Time => "Time:";
        public override string SoftWare => "Software:";
        public override string BugSubmit => "Bug submit";
        public override string MailServerCenter => "Mail Center System";
        public override string MailSendTail => "Mail Service system issued automatically, do not reply";
        public override string IpAddresError => "IP address input exception, format is incorrect";
        public override string Send => "Send";
        public override string Receive => "Receive";

        /***********************************************************************************
         * 
         *    System about
         * 
         ************************************************************************************/

        public override string SystemInstallOperater => "Install new software: ip address is";
        public override string SystemUpdateOperater => "Update software: ip address is";


        /***********************************************************************************
         * 
         *    Socket-related Information description
         * 
         ************************************************************************************/

        public override string SocketIOException => "Socket transport error: ";
        public override string SocketSendException => "Synchronous Data Send exception: ";
        public override string SocketHeadReceiveException => "Command header receive exception: ";
        public override string SocketContentReceiveException => "Content Data Receive exception: ";
        public override string NetClientAccountTimeout => "Wait for account check timeout：";
        public override string SocketContentRemoteReceiveException => "Recipient content Data Receive exception: ";
        public override string SocketAcceptCallbackException => "Asynchronously accepts an incoming connection attempt: ";
        public override string SocketReAcceptCallbackException => "To re-accept incoming connection attempts asynchronously";
        public override string SocketSendAsyncException => "Asynchronous Data send Error: ";
        public override string SocketEndSendException => "Asynchronous data end callback send Error";
        public override string SocketReceiveException => "Asynchronous Data send Error: ";
        public override string SocketEndReceiveException => "Asynchronous data end receive instruction header error";
        public override string SocketRemoteCloseException => "An existing connection was forcibly closed by the remote host";


        /***********************************************************************************
         * 
         *    File related information
         * 
         ************************************************************************************/


        public override string FileDownloadSuccess => "File Download Successful";
        public override string FileDownloadFailed => "File Download exception";
        public override string FileUploadFailed => "File Upload exception";
        public override string FileUploadSuccess => "File Upload Successful";
        public override string FileDeleteFailed => "File Delete exception";
        public override string FileDeleteSuccess => "File deletion succeeded";
        public override string FileReceiveFailed => "Confirm File Receive exception";
        public override string FileNotExist => "File does not exist";
        public override string FileSaveFailed => "File Store failed";
        public override string FileLoadFailed => "File load failed";
        public override string FileSendClientFailed => "An exception occurred when the file was sent";
        public override string FileWriteToNetFailed => "File Write Network exception";
        public override string FileReadFromNetFailed => "Read file exceptions from the network";
        public override string FilePathCreateFailed => "Folder path creation failed: ";
        public override string FileRemoteNotExist => "The other file does not exist, cannot receive!";

        /***********************************************************************************
         * 
         *    Engine-related data for the server
         * 
         ************************************************************************************/

        public override string TokenCheckFailed => "Receive authentication token inconsistency";
        public override string TokenCheckTimeout => "Receive authentication timeout: ";
        public override string CommandHeadCodeCheckFailed => "Command header check failed";
        public override string CommandLengthCheckFailed => "Command length check failed";
        public override string NetClientAliasFailed => "Client's alias receive failed: ";
        public override string NetEngineStart => "Start engine";
        public override string NetEngineClose => "Shutting down the engine";
        public override string NetClientOnline => "Online";
        public override string NetClientOffline => "Offline";
        public override string NetClientBreak => "Abnormal offline";
        public override string NetClientFull => "The server hosts the upper limit and receives an exceeded request connection.";
        public override string NetClientLoginFailed => "Error in Client logon: ";
        public override string NetHeartCheckFailed => "Heartbeat Validation exception: ";
        public override string NetHeartCheckTimeout => "Heartbeat verification timeout, force offline: ";
        public override string DataSourseFormatError => "Data source format is incorrect";
        public override string ServerFileCheckFailed => "Server confirmed file failed, please re-upload";
        public override string ClientOnlineInfo => "Client [ {0} ] Online";
        public override string ClientOfflineInfo => "Client [ {0} ] Offline";
        public override string ClientDisableLogin => "Client [ {0} ] is not trusted, login forbidden";

        /***********************************************************************************
         * 
         *    Client related
         * 
         ************************************************************************************/

        public override string ReConnectServerSuccess => "Re-connect server succeeded";
        public override string ReConnectServerAfterTenSeconds => "Reconnect the server after 10 seconds";
        public override string KeyIsNotAllowedNull => "The keyword is not allowed to be empty";
        public override string KeyIsExistAlready => "The current keyword already exists";
        public override string KeyIsNotExist => "The keyword for the current subscription does not exist";
        public override string ConnectingServer => "Connecting to Server...";
        public override string ConnectFailedAndWait => "Connection disconnected, wait {0} seconds to reconnect";
        public override string AttemptConnectServer => "Attempting to connect server {0} times";
        public override string ConnectServerSuccess => "Connection Server succeeded";
        public override string GetClientIpaddressFailed => "Client IP Address acquisition failed";
        public override string ConnectionIsNotAvailable => "The current connection is not available";
        public override string DeviceCurrentIsLoginRepeat => "ID of the current device duplicate login";
        public override string DeviceCurrentIsLoginForbidden => "The ID of the current device prohibits login";
        public override string PasswordCheckFailed => "Password validation failed";
        public override string DataTransformError => "Data conversion failed, source data: ";
        public override string RemoteClosedConnection => "Remote shutdown of connection";


        /***********************************************************************************
         * 
         *    Log related
         * 
         ************************************************************************************/
        public override string LogNetDebug => "Debug";
        public override string LogNetInfo => "Info";
        public override string LogNetWarn => "Warn";
        public override string LogNetError => "Error";
        public override string LogNetFatal => "Fatal";
        public override string LogNetAbandon => "Abandon";
        public override string LogNetAll => "All";

    }
}
