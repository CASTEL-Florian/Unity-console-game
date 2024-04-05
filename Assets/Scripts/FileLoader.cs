using System;
using System.IO;
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
            return (await LoadFileWebGL(path)).Split('\n');
         }

         return await File.ReadAllLinesAsync(path);
      }

      private static async UniTask<string> LoadFileWebGL(string filePath)
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