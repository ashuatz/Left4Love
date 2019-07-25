using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cubeat.DataTable
{
    using System.Reflection;
    using Core;
    using System;
    using System.IO;
    using System.Linq;

    public class ExcelDataManager : MonoSingleton<ExcelDataManager>
    {
        public enum State
        {
            None,
            InLoad,
            Loaded
        }

        public State _state { get; private set; }

        public event Action OnComplete = null;

        private readonly string[] _assetNames = new string[]
        {
            ""
        };

        private const string _assetPath = "Table/CSV";
        

        #region 테이블 데이터 로드 함수


        /// <summary>
        /// 모든 테이블 데이터 비동기 로드용 코루틴. 임시적으로 일단 Awake에서 호출됨. 호출위치는 수정가능
        /// </summary>
        /// <param name="onUpdate"> 진행도, 0 to 1</param>
        /// <returns></returns>
        public IEnumerator LoadDataAllAsync(Action<float> onUpdate = null)
        {
            //Logger.Log(LogType.Log, Instance.name, "LoadDataAllAsync called.");

            //Logger.LogFormat(LogType.Log, Instance.name, "LoadDataAllAsync - _state is '{0}'.", _state.ToString());

            if (_state != State.None)
            {
                while (_state != State.Loaded)
                {
                    yield return null;
                }

                if (onUpdate != null)
                {
                    onUpdate(1f);
                }

                yield break;
            }

            _state = State.InLoad;

            var length = _assetNames.Length;
            var value = 1f / length;

            //Logger.LogFormat(LogType.Log, Instance.name, "LoadDataAllAsync - length is '{0}'.", length);

            for (int i = 0; i < length; i++)
            {
                if (onUpdate != null)
                {
                    onUpdate(value * i);
                }

                yield return StartCoroutine(LoadDataAsync(_assetNames[i]));
            }

            _state = State.Loaded;

            onUpdate?.Invoke(1f);
            OnComplete?.Invoke();

            //Logger.Log(LogType.Log, Instance.name, "LoadDataAllAsync end.");
        }

        private IEnumerator LoadDataAsync(string assetName)
        {
            var methodName = string.Format("Load{0}Data", assetName);

            var methodInfo = GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);

            if (methodInfo == null)
            {
                yield break;
            }

            yield return StartCoroutine(LoadAssetAsync<TextAsset>(Application.dataPath, assetName, textAsset =>
            {
                if (textAsset == null || string.IsNullOrEmpty(textAsset.text))
                {
                    //Logger.LogFormat(LogType.Error, Instance.name, "textAsset is null!! assetName is {0}", assetName);

                    return;
                }

                var lineNumber = 0;

                using (var reader = new StringReader(textAsset.text))
                {
                    while (true)
                    {
                        var line = reader.ReadLine();

                        if (string.IsNullOrEmpty(line))
                        {
                            Logger.LogFormat(LogType.Log, Instance.name, "'{0}' in '{1}'. lineNumber is '{2}'.", assetName, Application.dataPath + "/" + _assetPath, lineNumber);

                            break;
                        }

                        lineNumber++;

                        methodInfo.Invoke(this, new object[] { line.Split('\t') });
                    }
                }
            }));
        }
        
        private IEnumerator LoadAssetAsync<T>(string assetPath, string assetName, Action<T> onLoaded) where T : UnityEngine.Object
        {
            //특정 환경(네트워크 엑세스 등)에서 확장될 수 있게 하기 위해 원래 해당함수는 비동기로 작성되었으나, 내부내용만 수정하였습니다.

            //이렇게 심플하게.
            T asset = Resources.Load<T>(_assetPath + "/" + assetName);

            if (onLoaded != null)
            {
                onLoaded(asset);
            }

            yield break;
        }

        #endregion
        
    }
}