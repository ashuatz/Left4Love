using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Core
{
    public class Subject
    {
        public event Action onNotify;

        public void Notify()
        {
            if (onNotify != null)
            {
                onNotify();
            }
        }

        public void Clear()
        {
            foreach (Action action in onNotify.GetInvocationList())
            {
                onNotify -= action;
            }
        }
    }

    public class Subject<T>
    {
        public event Action<T> onNotify;

        public event Action<T, T> onNotifyDelta;

        public void Notify(T t)
        {
            if (onNotify != null)
            {
                onNotify(t);
            }
        }

        public void Notify(T last, T current)
        {
            if (onNotifyDelta != null)
            {
                onNotifyDelta(last, current);
            }
        }

        public void Clear()
        {
            foreach (Action<T> action in onNotify.GetInvocationList())
            {
                onNotify -= action;
            }
        }
    }

    public class Subject<T, K>
    {
        public event Action<T, K> onNotify;

        public event Action<T, K, K> onNotifyDelta;

        public void Notify(T t, K k)
        {
            if (onNotify != null)
            {
                onNotify(t, k);
            }
        }

        public void Notify(T t, K last, K current)
        {
            if (onNotify != null)
            {
                onNotifyDelta(t, last, current);
            }
        }

        public void Clear()
        {
            foreach (Action<T, K> action in onNotify.GetInvocationList())
            {
                onNotify -= action;
            }
        }
    }

    public enum SubjectEventType
    {
        None,
        Add,
        Remove,
        Insert,
        Replace
    }

    public class SubjectValue<T>
    {
        Subject<T> _subject = new Subject<T>();

        T _value;

        bool _waitForSetValue = false;

        public event Action<T> onNotify
        {
            add { _subject.onNotify += value; }
            remove { _subject.onNotify -= value; }
        }

        public event Action<T, T> onNotifyDelta
        {
            add { _subject.onNotifyDelta += value; }
            remove { _subject.onNotifyDelta -= value; }
        }

        public T value
        {
            get { return _value; }
            set { Set(value, true); }
        }

        public SubjectValue() { }

        public SubjectValue(T value)
        {
            _value = value;
        }

        public void Set(T value, bool isNotify)
        {
            T last = _value;

            _value = value;

            if (isNotify)
            {
                _subject.Notify(_value);

                _subject.Notify(last, _value);
            }

            _waitForSetValue = false;
        }

        public IEnumerator WaitForSetValue(Action action)
        {
            _waitForSetValue = true;

            if (action != null)
                action();

            while (_waitForSetValue)
                yield return null;
        }
    }

    public class SubjectPrefsFloat
    {
        Subject<float> _subject = new Subject<float>();
        public Subject<float> subject { get { return _subject; } }

        readonly string _key;
        readonly float _defaultValue;

        public SubjectPrefsFloat(string key, float defaultValue)
        {
            _key = key;
            _defaultValue = defaultValue;
        }

        public float value
        {
            get { return PlayerPrefs.GetFloat(_key, _defaultValue); }
            set
            {
                PlayerPrefs.SetFloat(_key, value);
                _subject.Notify(value);
            }
        }
    }

    public class SubjectPrefsInt
    {
        Subject<int> _subject = new Subject<int>();
        public Subject<int> subject { get { return _subject; } }

        readonly string _key;
        readonly int _defaultValue;

        public SubjectPrefsInt(string key, int defaultValue)
        {
            _key = key;
            _defaultValue = defaultValue;
        }

        public int value
        {
            get { return PlayerPrefs.GetInt(_key, _defaultValue); }
            set
            {
                PlayerPrefs.SetInt(_key, value);
                _subject.Notify(value);
            }
        }
    }

    public class SubjectPrefsString
    {
        Subject<string> _subject = new Subject<string>();
        public Subject<string> subject { get { return _subject; } }

        readonly string _key;
        readonly string _defaultValue;

        public SubjectPrefsString(string key, string defaultValue)
        {
            _key = key;
            _defaultValue = defaultValue;
        }

        public string value
        {
            get { return PlayerPrefs.GetString(_key, _defaultValue); }
            set
            {
                PlayerPrefs.SetString(_key, value);
                _subject.Notify(value);
            }
        }
    }
}