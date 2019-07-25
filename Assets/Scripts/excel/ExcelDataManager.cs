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

        private readonly string[] _assetNames = new string[]
        {
            "GlobalWeightTable", "RhythmTable", "SceneTable","CharTable","ObjectTable", "ProducingTable", "SoundBeatTable"
            , "GameModeTable", "SoundChangeTable"
        };

        private const string _assetPath = "Table/CSV";

        private Dictionary<int,GlobalWeightTable> _globalWeightTable = new Dictionary<int, GlobalWeightTable>();
        private Dictionary<int, RhythmTable> _rhythmTable = new Dictionary<int, RhythmTable>();
        private Dictionary<int, SceneTable> _sceneTable = new Dictionary<int, SceneTable>();
        private Dictionary<int, CharTable> _charTable = new Dictionary<int, CharTable>();
        private Dictionary<int, ObjectTable> _objectTable = new Dictionary<int, ObjectTable>();
        private Dictionary<int, ProducingTable> _producingTable = new Dictionary<int, ProducingTable>();
        private Dictionary<int, SoundBeatTable> _soundBeatTable = new Dictionary<int, SoundBeatTable>();
        private Dictionary<int, GameModeTable> _gameModeTable = new Dictionary<int, GameModeTable>();
        private Dictionary<int, SoundChangeTable> _soundChangeTable = new Dictionary<int, SoundChangeTable>();

        public State _state { get; private set; }
        public event Action OnComplete;
        
        //데이터 접근
        public Dictionary<int, GlobalWeightTable> GetGlobalWeightDic()
        {
            return _globalWeightTable;
        }

        public Dictionary<int, RhythmTable> GetRhythmDic()
        {
            return _rhythmTable;
        }

        public SceneTable GetScene(int idx)
        {
            return _sceneTable.ContainsKey(idx) ? _sceneTable[idx] : null;
        }

        public Dictionary<int, SceneTable> GetSceneDic()
        {
            return _sceneTable;
        }

        public Dictionary<int,CharTable> GetCharDic()
        {
            return _charTable;
        }

        public Dictionary<int, ObjectTable> GetObjectDic()
        {
            return _objectTable;
        }

        public Dictionary<int, ProducingTable> GetProducingDic()
        {
            return _producingTable;
        }

        public Dictionary<int, SoundBeatTable> GetSoundBeatDic()
        {
            return _soundBeatTable;
        }

        public Dictionary<int, GameModeTable> GetGameModeDic()
        {
            return _gameModeTable;
        }

        public Dictionary<int, SoundChangeTable> GetSoundChangeDic()
        {
            return _soundChangeTable;
        }

        public GameModeTable GetGameModeData(GameModeType gameModeType)
        {
            return _gameModeTable[(int)gameModeType];
        }

        //데이터 간편접근 
        public GlobalWeightTable GetGlobal(int idx)
        {
            return _globalWeightTable.ContainsKey(idx) ? _globalWeightTable[idx] : null;
        }

        public GlobalWeightTable GetGlobal(string name)
        {
            return _globalWeightTable.FirstOrDefault((data => data.Value._Name.Equals(name))).Value;
        }

        public RhythmTable GetRhythm(int idx)
        {
            return _rhythmTable.ContainsKey(idx) ? _rhythmTable[idx] : null;
        }

        public RhythmTable GetRhythm(RhythmType type)
        {
            return _rhythmTable.FirstOrDefault((kvp) => kvp.Value._Type == type).Value;
        }

        public RhythmTable GetRhythm(Func<KeyValuePair<int, RhythmTable>, bool> predicate)
        {
            return _rhythmTable.FirstOrDefault(predicate).Value;
        }

        public IList<RhythmTable> GetRhythms(Func<KeyValuePair<int, RhythmTable>, bool> predicate)
        {
            List<RhythmTable> tables = new List<RhythmTable>();
            var e = _rhythmTable.GetEnumerator();
            while(e.MoveNext())
            {
                if(predicate(e.Current))
                {
                    tables.Add(e.Current.Value);
                }
            }

            return tables;
        }

        public CharTable GetChar(Func<KeyValuePair<int,CharTable>,bool> predicate)
        {
            return _charTable.FirstOrDefault(predicate).Value;
        }

        public ObjectTable GetObject(int idx)
        {
            return _objectTable.ContainsKey(idx) ? _objectTable[idx] : null;
        }

        public ObjectTable GetObject(Func<KeyValuePair<int, ObjectTable>, bool> predicate)
        {
            return _objectTable.FirstOrDefault(predicate).Value;
        }

        public ObjectTable GetObject(ObjectType type)
        {
            return _objectTable.FirstOrDefault((kvp) => kvp.Value._Type == type).Value;
        }

        //config 값 한번에 읽기
        public ProducingTable GetProducingData()
        {
            return _producingTable.First().Value;
        }


        //데이터 로드 함수 * 중요
        private void LoadGlobalWeightTableData(string[] parts)
        {
            var p = GlobalWeightTable.Load(parts);

            _globalWeightTable.Add(p._Index, p);
        }
        private void LoadRhythmTableData(string[] parts)
        {
            var p = RhythmTable.Load(parts);

            _rhythmTable.Add(p._Index, p);
        }

        private void LoadSceneTableData(string[] parts)
        {
            var p = SceneTable.Load(parts);

            _sceneTable.Add(p._Index, p);
        }
        
        private void LoadCharTableData(string[] parts)
        {
            var p = CharTable.Load(parts);

            _charTable.Add(p._index, p);
        }

        private void LoadObjectTableData(string[] parts)
        {
            var p = ObjectTable.Load(parts);

            _objectTable.Add(p._Index, p);
        }

        private void LoadProducingTableData(string[] parts)
        {
            var p = ProducingTable.Load(parts);

            _producingTable.Add(p._Index, p);
        }

        private void LoadSoundBeatTableData(string[] parts)
        {
            var p = SoundBeatTable.Load(parts);

            _soundBeatTable.Add(p._Index, p);
        }

        private void LoadGameModeTableData(string[] parts)
        {
            var p = GameModeTable.Load(parts);

            _gameModeTable.Add(p._Index, p);
        }

        private void LoadSoundChangeTableData(string[] parts)
        {
            var p = SoundChangeTable.Load(parts);

            _soundChangeTable.Add(p._Index, p);
        }

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