using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace UnityConsole
{
   public static class FileLoader
   {
      public static async UniTask<string[]> ReadAllLinesAsync(string path)
      {
         if (Application.platform == RuntimePlatform.WebGLPlayer)
         {
            return (await LoadFileTextWebGL(path)).Split('\n').Select((x) => x.Trim('\r')).ToArray();
         }

         return await File.ReadAllLinesAsync(path);
      }
      
      public static async UniTask<string> ReadAllTextAsync(string path)
      {
         if (Application.platform == RuntimePlatform.WebGLPlayer)
         {
            return await LoadFileTextWebGL(path);
         }

         return await File.ReadAllTextAsync(path);
      }
      
      public static async UniTask<byte[]> ReadAllBytesAsync(string path)
      {
         if (Application.platform == RuntimePlatform.WebGLPlayer)
         {
            return await LoadFileBytesWebGL(path);
         }

         return await File.ReadAllBytesAsync(path);
      }

      private static async UniTask<string> LoadFileTextWebGL(string filePath)
      {
         using (UnityWebRequest webRequest =
                UnityWebRequest.Get(filePath))
         {
            await webRequest.SendWebRequest();
            if (webRequest.result != UnityWebRequest.Result.Success)
            {
               Debug.LogError(webRequest.error);
               return "";
            }
            return webRequest.downloadHandler.text;
         }
      }
      
      private static async UniTask<byte[]> LoadFileBytesWebGL(string filePath)
      {
         using (UnityWebRequest webRequest =
                UnityWebRequest.Get(filePath))
         {
            await webRequest.SendWebRequest();
            if (webRequest.result != UnityWebRequest.Result.Success)
            {
               Debug.LogError(webRequest.error);
               return Array.Empty<byte>();
            }
            return webRequest.downloadHandler.data;
         }
      }
   }
   
   
   public class UnityWebRequestAwaiter : INotifyCompletion
   {
      private UnityWebRequestAsyncOperation asyncOp;
      private Action continuation;

      public UnityWebRequestAwaiter(UnityWebRequestAsyncOperation asyncOp)
      {
         this.asyncOp = asyncOp;
         asyncOp.completed += OnRequestCompleted;
      }

      public bool IsCompleted { get { return asyncOp.isDone; } }

      public void GetResult() { }

      public void OnCompleted(Action continuation)
      {
         this.continuation = continuation;
      }

      private void OnRequestCompleted(AsyncOperation obj)
      {
         continuation();
      }
   }

   public static class UnityWebRequestExtensionMethods
   {
      public static UnityWebRequestAwaiter GetAwaiter(this UnityWebRequestAsyncOperation asyncOp)
      {
         return new UnityWebRequestAwaiter(asyncOp);
      }
   }
}