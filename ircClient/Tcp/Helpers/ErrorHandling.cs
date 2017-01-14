/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
namespace ircClient.Tcp.Helpers
{
    public enum WsaErrorConstants
    {
        WsaBaseErr = 10000,
        WsaErrIntr = (WsaBaseErr + 4),
        WsaErrAccess = (WsaBaseErr + 13),
        WsaErrFault = (WsaBaseErr + 14),
        WsaErrInvalid = (WsaBaseErr + 22),
        WsaErrTooManyFiles = (WsaBaseErr + 24),
        WsaErrWouldBlock = (WsaBaseErr + 35),
        WsaErrInProgress = (WsaBaseErr + 36),
        WsaErrAlready = (WsaBaseErr + 37),
        WsaErrNonSock = (WsaBaseErr + 38),
        WsaErrDestAddrReq = (WsaBaseErr + 39),
        WsaErrMsgSize = (WsaBaseErr + 40),
        WsaErrProtoType = (WsaBaseErr + 41),
        WsaErrNoProtoOpt = (WsaBaseErr + 42),
        WsaErrProtoNoSupport = (WsaBaseErr + 43),
        WsaErrSockTypeNoSupport = (WsaBaseErr + 44),
        WsaErrOpNoSupport = (WsaBaseErr + 45),
        WsaErrProtFamNoSupport = (WsaBaseErr + 46),
        WsaErrAdrrFamNoSupport = (WsaBaseErr + 47),
        WsaErrAddrInUse = (WsaBaseErr + 48),
        WsaErrAddrNotAvail = (WsaBaseErr + 49),
        WsaErrNetDown = (WsaBaseErr + 50),
        WsaErrNetUnreach = (WsaBaseErr + 51),
        WsaErrNetReset = (WsaBaseErr + 52),
        WsaEConnAborted = (WsaBaseErr + 53),
        WsaErrConnReset = (WsaBaseErr + 54),
        WsaErrNoBufs = (WsaBaseErr + 55),
        WsaErrIsConn = (WsaBaseErr + 56),
        WsaErrNotConn = (WsaBaseErr + 57),
        WsaErrShutdown = (WsaBaseErr + 58),
        WsaErrTimedOut = (WsaBaseErr + 60),
        WsaErrHostUnreach = (WsaBaseErr + 65),
        WsaEConnRefused = (WsaBaseErr + 61),
        WsaErrProcLim = (WsaBaseErr + 67),
        WsaSysNotReady = (WsaBaseErr + 91),
        WsaVerNotSupported = (WsaBaseErr + 92),
        WsaNotInitialised = (WsaBaseErr + 93),
        WsaHostNotFound = (WsaBaseErr + 1001),
        WsaTryAgain = (WsaBaseErr + 1002),
        WsaNoRecovery = (WsaBaseErr + 1003),
        WsaNoData = (WsaBaseErr + 1004)
    }

    public static class ErrorHandling
    {
        public static string GetErrorDescription(int errorCode)
        {
            switch (errorCode)
            {
                case (int)WsaErrorConstants.WsaErrAccess:
                    return "Permission denied";

                case (int)WsaErrorConstants.WsaErrAddrInUse:
                    return "Address already in use";

                case (int)WsaErrorConstants.WsaErrAddrNotAvail:
                    return "Cannot assign requested address";

                case (int)WsaErrorConstants.WsaErrAdrrFamNoSupport:
                    return "Address family not supported by protocol family";

                case (int)WsaErrorConstants.WsaErrAlready:
                    return "Operation already in progress";

                case (int)WsaErrorConstants.WsaEConnAborted:
                    return "Software caused connection abort";

                case (int)WsaErrorConstants.WsaEConnRefused:
                    return "Connection refused";

                case (int)WsaErrorConstants.WsaErrConnReset:
                    return "Connection reset by peer";

                case (int)WsaErrorConstants.WsaErrDestAddrReq:
                    return "Destination address required";

                case (int)WsaErrorConstants.WsaErrFault:
                    return "Bad address";

                case (int)WsaErrorConstants.WsaErrHostUnreach:
                    return "No route to host";

                case (int)WsaErrorConstants.WsaErrInProgress:
                    return "Operation now in progress";

                case (int)WsaErrorConstants.WsaErrIntr:
                    return "Interrupted function call";

                case (int)WsaErrorConstants.WsaErrInvalid:
                    return "Invalid argument";

                case (int)WsaErrorConstants.WsaErrIsConn:
                    return "Socket is already connected";

                case (int)WsaErrorConstants.WsaErrTooManyFiles:
                    return "Too many open files";

                case (int)WsaErrorConstants.WsaErrMsgSize:
                    return "Message too long";

                case (int)WsaErrorConstants.WsaErrNetDown:
                    return "Network is down";

                case (int)WsaErrorConstants.WsaErrNetReset:
                    return "Network dropped connection on reset";

                case (int)WsaErrorConstants.WsaErrNetUnreach:
                    return "Network is unreachable";

                case (int)WsaErrorConstants.WsaErrNoBufs:
                    return "No buffer space available";

                case (int)WsaErrorConstants.WsaErrNoProtoOpt:
                    return "Bad protocol option";

                case (int)WsaErrorConstants.WsaErrNotConn:
                    return "Socket is not connected";

                case (int)WsaErrorConstants.WsaErrNonSock:
                    return "Socket operation on non-socket";

                case (int)WsaErrorConstants.WsaErrOpNoSupport:
                    return "Operation not supported.";

                case (int)WsaErrorConstants.WsaErrProtFamNoSupport:
                    return "Protocol family not supported";

                case (int)WsaErrorConstants.WsaErrProcLim:
                    return "Too many processes";

                case (int)WsaErrorConstants.WsaErrProtoNoSupport:
                    return "Protocol not supported";

                case (int)WsaErrorConstants.WsaErrProtoType:
                    return "Protocol wrong type for socket";

                case (int)WsaErrorConstants.WsaErrShutdown:
                    return "Cannot send after socket shutdown";

                case (int)WsaErrorConstants.WsaErrSockTypeNoSupport:
                    return "Socket type not supported";

                case (int)WsaErrorConstants.WsaErrTimedOut:
                    return "Connection timed out";

                case (int)WsaErrorConstants.WsaErrWouldBlock:
                    return "Resource temporarily unavailable";

                case (int)WsaErrorConstants.WsaHostNotFound:
                    return "Host not found";

                case (int)WsaErrorConstants.WsaNotInitialised:
                    return "Successful WSAStartup not yet performed";

                case (int)WsaErrorConstants.WsaNoData:
                    return "Valid name, no data record of requested type";

                case (int)WsaErrorConstants.WsaNoRecovery:
                    return "This is a non-recoverable error";

                case (int)WsaErrorConstants.WsaSysNotReady:
                    return "Network subsystem is unavailable";

                case (int)WsaErrorConstants.WsaTryAgain:
                    return "Nonauthoritative host not found";

                case (int)WsaErrorConstants.WsaVerNotSupported:
                    return "Winsock.dll version out of range";

                default:
                    return "Unknown error";
            }
        }
    }
}
