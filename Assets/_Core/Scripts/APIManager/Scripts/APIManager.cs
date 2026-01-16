using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class APIManager : MonoBehaviour
{
     [SerializeField] private APIEndpointConfig config;
     private Dictionary<APIEndpointConfig.APIEndpointType, string> endpointMap = new();
     private static APIManager instance;

     public static APIManager Instance
     {
         get
         {
             if (instance == null)
             {
                 instance = FindFirstObjectByType<APIManager>();
             }
             return instance;
         }
     }

     [System.Serializable]
     private class LoginRequestData
     {
         public string cpf;
         public string password;
     }

     private void Awake()
     {
         DontDestroyOnLoad(gameObject);
         BuildEndpointMap();
     }

     public async Task<UserData> Login(string cpf, string password)
     {
         var endpointType = APIEndpointConfig.APIEndpointType.Login;
         var path = GetEndpointPath(endpointType);
         var url = $"{config.baseURL}{path}";

         var loginData = new LoginRequestData
         {
             cpf = cpf,
             password = password
         };

         var jsonBody = JsonUtility.ToJson(loginData);
         var rawBody = Encoding.UTF8.GetBytes(jsonBody);

         using var request = new UnityWebRequest(url, "POST");
         request.uploadHandler = new UploadHandlerRaw(rawBody);
         request.downloadHandler = new DownloadHandlerBuffer();
         request.SetRequestHeader("Content-Type", "application/json");
         request.timeout = config.timeout;

         try
         {
             await request.SendWebRequest();
             if (request.result != UnityWebRequest.Result.Success)
             {
                 // HandleApiErrorResponse(request, endpointType);
                 return null;
             }

             var successJson = request.downloadHandler.text;
             var loginResponse = JsonUtility.FromJson<UserData>(successJson);

             if (loginResponse != null && loginResponse.sessions.Count > 0 && !string.IsNullOrEmpty(loginResponse.sessions[0].token))
             {
                 return loginResponse;
             }
             
             ShowGenericErrorPopup("Erro de Resposta", "A resposta do servidor não pôde ser processada corretamente.");
             return null;
         }
         catch (Exception ex)
         {
             throw HandleError(ex, request, endpointType);
         }
     }

     public async Task<UnityWebRequest> Post<T>(APIEndpointConfig.APIEndpointType endpointType, T data, string lastRoute = "")
     {
         var path = GetEndpointPath(endpointType);

         var url = "";
         url = config.endpoints.First(c => c.endpointType == endpointType).useBaseEndpoint ? $"{config.baseURL}{path}" : $"{path}";
         
         if (!string.IsNullOrEmpty(lastRoute) && path.Contains("{}"))
         {
             url = url.Replace("{}", lastRoute);
         }

         var jsonBody = JsonUtility.ToJson(data);
         var rawBody = Encoding.UTF8.GetBytes(jsonBody);
         
         var request = new UnityWebRequest(url, "POST");
         request.uploadHandler = new UploadHandlerRaw(rawBody);
         request.downloadHandler = new DownloadHandlerBuffer();
         request.SetRequestHeader("Content-Type", "application/json");
         request.timeout = config.timeout;

         if (config.endpoints.First(c => c.endpointType == endpointType).useToken)
         {
             var authToken = UserManager.Instance.Token;
             request.SetRequestHeader("Authorization", $"Bearer {authToken}");
         }

         try
         {
             await request.SendWebRequest();

             if (request.result != UnityWebRequest.Result.Success)
             {
                 HandleApiErrorResponse(request, endpointType);
                 return null;
             }
             
             return request;
         }
         catch (Exception ex)
         {
             request.Dispose(); 
             throw HandleError(ex, request, endpointType);
         }
     }
    

     public async Task<string> Get(APIEndpointConfig.APIEndpointType endpointType, string lastRoute = "")
     {
         var path = GetEndpointPath(endpointType);
         
         var url = "";
         url = config.endpoints.First(c => c.endpointType == endpointType).useBaseEndpoint ? $"{config.baseURL}{path}" : $"{path}";
         
         if (!string.IsNullOrEmpty(lastRoute) && path.Contains("{}"))
         {
             url = url.Replace("{}", lastRoute);
         }
         
         using var request = new UnityWebRequest(url, "GET");
         request.downloadHandler = new DownloadHandlerBuffer();
         request.timeout = config.timeout;
         
         if (config.endpoints.First(c => c.endpointType == endpointType).useToken)
         {
             var authToken = UserManager.Instance.Token;
             request.SetRequestHeader("Authorization", $"Bearer {authToken}");
         }
         
         try
         {
             await request.SendWebRequest();
          
             if (request.result != UnityWebRequest.Result.Success)
             {
                 HandleApiErrorResponse(request, endpointType);
                 return null;
             }
             
             return request.downloadHandler.text;
         }
         catch (Exception ex)
         {
             throw HandleError(ex, request, endpointType);
         }
     }
     
     /// <summary>
     /// Baixa uma textura de uma URL.
     /// </summary>
     /// <param name="url">A URL completa da imagem.</param>
     /// <returns>A Texture2D baixada, ou null em caso de falha.</returns>
     public async Task<Texture2D> GetTextureFromUrl(string url)
     {
         using var request = new UnityWebRequest(url);
         request.downloadHandler = new DownloadHandlerTexture();
         request.timeout = config.timeout;
         
         // var authToken = UserManager.Instance.Token; 
         // if (!string.IsNullOrEmpty(authToken))
         // {
         //     request.SetRequestHeader("Authorization", $"Bearer {authToken}");
         // }
         // else
         // {
         //     Debug.LogWarning("Tentando fazer uma requisição autenticada de imagem, mas não há token de usuário.");
         // }

         try
         {
             await request.SendWebRequest();

             if (request.result != UnityWebRequest.Result.Success)
             {
                 Debug.LogError($"[API Error] Falha ao baixar textura de {url}. Código: {request.responseCode}. Erro: {request.error}");
                 return null;
             }

             return DownloadHandlerTexture.GetContent(request);
         }
         catch(Exception ex)
         {
             Debug.LogError($"Uma exceção ocorreu ao baixar a textura: {ex.Message}");
             return null;
         }
     }
     
     public async Task<UnityWebRequest> Patch<T>(APIEndpointConfig.APIEndpointType endpointType, T data, string id)
     {
         var path = GetEndpointPath(endpointType);
         
         var url = "";
         url = config.endpoints.First(c => c.endpointType == endpointType).useBaseEndpoint ? $"{config.baseURL}{path}" : $"{path}";
         
         if (!string.IsNullOrEmpty(id) && path.Contains("{}"))
         {
             url = url.Replace("{}", id);
         }

         var jsonBody = JsonUtility.ToJson(data);
         var rawBody = Encoding.UTF8.GetBytes(jsonBody);
         
         var request = new UnityWebRequest(url, "PATCH");
         request.uploadHandler = new UploadHandlerRaw(rawBody);
         request.downloadHandler = new DownloadHandlerBuffer();
         request.SetRequestHeader("Content-Type", "application/json");
         request.timeout = config.timeout;

         if (config.endpoints.First(c => c.endpointType == endpointType).useToken)
         {
             var authToken = UserManager.Instance.Token;
             request.SetRequestHeader("Authorization", $"Bearer {authToken}");
         }

         try
         {
             await request.SendWebRequest();

             if (request.result != UnityWebRequest.Result.Success)
             {
                 HandleApiErrorResponse(request, endpointType);
                 return null;
             }
             
             return request;
         }
         catch (Exception ex)
         {
             request.Dispose();
             throw HandleError(ex, request, endpointType);
         }
     }
     
     public async Task<UnityWebRequest> Delete(APIEndpointConfig.APIEndpointType endpointType, string id)
     {
         var path = GetEndpointPath(endpointType);
         var url = $"{config.baseURL}{path}";
         if (!string.IsNullOrEmpty(id) && path.Contains("{}"))
         {
             url = url.Replace("{}", id);
         }
         
         using var request = new UnityWebRequest(url, "DELETE");
         request.downloadHandler = new DownloadHandlerBuffer();
         request.timeout = config.timeout;
         
         if (config.endpoints.FirstOrDefault(c => c.endpointType == endpointType).useToken)
         {
             var authToken = UserManager.Instance.Token;
             request.SetRequestHeader("Authorization", $"Bearer {authToken}");
         }

         try
         {
             await request.SendWebRequest();

             if (request.result != UnityWebRequest.Result.Success)
             {
                 HandleApiErrorResponse(request, endpointType);
                 return null;
             }
             

             return request;
         }
         catch (Exception ex)
         {
             request.Dispose();
             throw HandleError(ex, request, endpointType);
         }
     }

     private void BuildEndpointMap()
     {
         endpointMap.Clear();
         foreach (var endpoint in config.endpoints)
         {
             if (endpointMap.ContainsKey(endpoint.endpointType))
             {
                 continue;
             }
             endpointMap[endpoint.endpointType] = endpoint.path;
         }
     }

     private string GetEndpointPath(APIEndpointConfig.APIEndpointType endpointType)
     {
         if (endpointMap.TryGetValue(endpointType, out var path))
         {
             return path;
         }
         throw new ArgumentException($"Endpoint {endpointType} not configured in APIConfig");
     }

     private Exception HandleError(Exception ex, UnityWebRequest request, APIEndpointConfig.APIEndpointType endpointType)
     {
         var errorMessage = $"[API Error] {endpointType}\n" +
                            $"URL: {request.url}\n" +
                            $"Error: {request.error}\n" +
                            $"Code: {request.responseCode}\n" +
                            $"Response: {request.downloadHandler?.text}";
         return new Exception(errorMessage, ex);
     }

     private void HandleApiErrorResponse(UnityWebRequest request, APIEndpointConfig.APIEndpointType endpointType)
     {
         string errorJson = request.downloadHandler.text;
         string errorMessageForPopup = 
             $"Ocorreu um erro inesperado. (Endpoint: {endpointType}, Código: {request.responseCode})";

         if (!string.IsNullOrEmpty(errorJson))
         {
             try
             {
                 // Tenta deserializar o JSON para a sua classe ApiErrorResponse
                 var errorResponse = JsonUtility.FromJson<ApiErrorResponse>(errorJson);

                 // Verifica se a lista de mensagens não está vazia
                 if (errorResponse?.message != null && errorResponse.message.Count > 0)
                 {
                     // Concatena todas as mensagens da lista, cada uma em uma nova linha
                     errorMessageForPopup = string.Join("\n", errorResponse.message);
                 }
                 // Se a lista de mensagens estiver vazia, usa o campo 'error' como fallback
                 else if (!string.IsNullOrEmpty(errorResponse.error))
                 {
                     errorMessageForPopup = errorResponse.error;
                 }
             }
             catch (Exception)
             {
                 // Se o parse do JSON falhar, usa o erro genérico da requisição
                 if (!string.IsNullOrEmpty(request.error))
                 {
                     errorMessageForPopup = request.error;
                 }
             }
         }
         else if (!string.IsNullOrEmpty(request.error))
         {
             // Se não houver corpo de resposta, usa o erro da requisição (ex: falha de rede)
             errorMessageForPopup = request.error;
         }

         // Monta e exibe o popup de erro para o usuário
         var popupData = new PopUpData
         {
             Header = "Opa, algo aconteceu!",
             PopUpType = EPopUpType.Ok,
             Description = errorMessageForPopup
         };
    
         // PopUpManager.Instance.SendCustomPopUp(popupData, null);
     }

     private void ShowGenericErrorPopup(string header, string description)
     {
         var popupData = new PopUpData
         {
             Header = header,
             PopUpType = EPopUpType.Ok,
             Description = description
         };
         // PopUpManager.Instance.SendCustomPopUp(popupData, null);
     }
}