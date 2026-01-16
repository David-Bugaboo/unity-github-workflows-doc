using System;

public static class ErrorHandler {
    //public static event Action<ErrorData> OnError;
    public static event Action<string> OnError;
    /*
    public static void ShowErrorPlayfab( PlayFabError err, string msg ) {
        if ( err == null ) return;
        OnError?.Invoke( new() { Message = msg } );
        Log( $"Error - Code: {err.HttpCode}\nMessage: {err.ErrorMessage}\nAdditional: {err.ApiEndpoint},{err.GenerateErrorReport()}" );
    }
    public static void ShowError( ErrorData data ) {
        OnError?.Invoke( data );
        Log( $"Error - Title: {data.Title}\nMessage: {data.Message}\nCode: {data.Code}" );
    }
    */

    public static void ShowError( string id ) {
        OnError?.Invoke( id );
        //Log( $"Error - Title: {data.Title}\nMessage: {data.Message}\nCode: {data.Code}" );
    }

    public class ErrorData
    {
        public string Title, Message, Code;
        public ErrorData()
        { }
    }
}
