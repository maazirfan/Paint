﻿using Leap.Unity.Query;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.UserContext {

  using UnityObject = UnityEngine.Object;

  public enum ContextDescriptionType {
    UserModel = 0,
    UnityObject = 1,
  }

  #region Some Unity-serializable UconChannel Wrapper Classes

  /// <summary>
  /// UconChannel pre-defines channels for a few common types.
  /// If a type you need to use in a channel doesn't already exist, create a new
  /// subclass of UconChannel with the type argument you need.
  /// </summary>
  [Serializable]
  public class FloatChannel : UconChannel<float> { }

  /// <summary>
  /// UconChannel pre-defines channels for a few common types.
  /// If a type you need to use in a channel doesn't already exist, create a new
  /// subclass of UconChannel with the type argument you need.
  /// </summary>
  [Serializable]
  public class IntChannel : UconChannel<int> { }

  /// <summary>
  /// UconChannel pre-defines channels for a few common types.
  /// If a type you need to use in a channel doesn't already exist, create a new
  /// subclass of UconChannel with the type argument you need.
  /// </summary>
  [Serializable]
  public class Vector2Channel : UconChannel<Vector2> { }

  /// <summary>
  /// UconChannel pre-defines channels for a few common types.
  /// If a type you need to use in a channel doesn't already exist, create a new
  /// subclass of UconChannel with the type argument you need.
  /// </summary>
  [Serializable]
  public class Vector3Channel : UconChannel<Vector3> { }

  /// <summary>
  /// UconChannel pre-defines channels for a few common types.
  /// If a type you need to use in a channel doesn't already exist, create a new
  /// subclass of UconChannel with the type argument you need.
  /// </summary>
  [Serializable]
  public class Vector4Channel : UconChannel<Vector4> { }

  /// <summary>
  /// UconChannel pre-defines channels for a few common types.
  /// If a type you need to use in a channel doesn't already exist, create a new
  /// subclass of UconChannel with the type argument you need.
  /// </summary>
  [Serializable]
  public class QuaternionChannel : UconChannel<Quaternion> { }

  /// <summary>
  /// UconChannel pre-defines channels for a few common types.
  /// If a type you need to use in a channel doesn't already exist, create a new
  /// subclass of UconChannel with the type argument you need.
  /// </summary>
  [Serializable]
  public class HandChannel : UconChannel<Hand> { }

  /// <summary>
  /// UconChannel pre-defines channels for a few common types.
  /// If a type you need to use in a channel doesn't already exist, create a new
  /// subclass of UconChannel with the type argument you need.
  /// </summary>
  [Serializable]
  public class PoseChannel : UconChannel<Pose> { }

  /// <summary>
  /// UconChannel pre-defines channels for a few common types.
  /// If a type you need to use in a channel doesn't already exist, create a new
  /// subclass of UconChannel with the type argument you need.
  /// </summary>
  [Serializable]
  public class ColorChannel : UconChannel<Color> { }

  #endregion

  [Serializable]
  public abstract class UconChannel {

    public const UserContextType DEFAULT_USER_CONTEXT_TYPE = UserContextType.Local;

    /// <summary>
    /// The data type of this channel.
    /// </summary>
    public abstract Type channelType { get; }

    [SerializeField]
    private ContextDescriptionType _contextType;
    /// <summary>
    /// Whether the channel context is specified by a Unity object or a User Context
    /// Type enum value.
    /// </summary>
    public ContextDescriptionType contextType { get { return _contextType; } }

    /// <summary>
    /// The UserContextTy[e to use as the context for this channel, if the context type
    /// is set to ContextDescriptionType.UserModel.
    /// </summary>
    [SerializeField]
    private UserContextType _userContextType = UserContextType.Local;
    public UserContextType userContextType { get { return _userContextType; } }

    [SerializeField]
    private UnityObject _contextObj = null;
    /// <summary>
    /// The Unity object to use as the context for this channel, if the context type is
    /// set to ContextDescriptionType.CustomUnityObject.
    /// </summary>
    public UnityObject contextObj { get { return _contextObj; } }

    [SerializeField]
    private string _channelPath = "";
    public string channelPath {
      get {
        if (_channelPath == null) { _channelPath = ""; }
        return _channelPath;
      }
    }

  }

  /// <summary>
  /// A Ucon channel description, consisting of a context object, path string, and data
  /// type (the type argument.
  /// 
  /// A given triplet (context, channel path, type) will refer to the same memory, which
  /// is a List containing zero or more values of the channel data type.
  /// 
  /// To serialize a UconChannel for display in the inspector, use one of the pre-defined
  /// typed implementations such as UconChannel_float or UconChannel_MonoBehaviour, or
  /// create a new class that implements UconChannel for your type (no implementation
  /// necessary), in the same way you would for a typed UnityEvent.
  /// </summary>
  [Serializable]
  public class UconChannel<T> : UconChannel {

    /// <summary>
    /// The data type of this channel. This is primarily used by the custom property
    /// drawer for Ucon channels.
    /// </summary>
    public override Type channelType { get { return typeof(T); } }

    private List<T> _backingSource;
    /// <summary>
    /// Gets the underlying buffer resource, a generic List of the channel data type.
    /// This is a lazily-constructed List, unique to a triplet of specifiers for the
    /// channel: A context object, a channel path string, and the channel data type.
    /// </summary>
    public List<T> source {
      get {
        if (_backingSource == null) {
          _backingSource = Ucon.Context(contextObj).At(channelPath).GetBuffer<T>();
        }
        return _backingSource;
      }
    }

    // UconChannel<T> API:
    //
    // uconChannel.Get();
    // uconChannel.GetIsEmpty();
    // uconChannel.TryGet(out Color c) -> bool
    // uconChannel.GetEach()
    // uconChannel.Query()
    // uconChannel.Set(Color c);
    // uconChannel.Set(IList<Color> c);
    // uconChannel.Clear();
    // uconChannel.Add(Color c);
    // uconChannel.Add(IList<Color> c);
    // uconChannel.Remove(Color c);

    #region Reads

    /// <summary>
    /// Returns the currently stored value at this channel, or the first value if there
    /// is more than one.
    /// 
    /// If there is no data at this channel yet, returns the default value for the type.
    /// </summary>
    public T Get() {
      return source.Query().FirstOrDefault();
    }

    /// <summary>
    /// Returns the current number of values at this channel.
    /// </summary>
    public int Count {
      get { return source.Count; }
    }

    /// <summary>
    /// Gets whether there is any data at this channel.
    /// </summary>
    public bool IsEmpty {
      get { return Count == 0; }
    }

    /// <summary>
    /// Returns true if there is at least one value at the channel and outputs that value.
    /// Otherwise returns false, and the output is the default value for the type.
    /// </summary>
    public bool TryGet(out T value) {
      if (source.Count == 0) {
        value = default(T);
        return false;
      }
      value = source[0];
      return true;
    }

    /// <summary>
    /// Returns a Queryable object on the data in this channel.
    /// </summary>
    public QueryWrapper<T, QueryConversionExtensions.ListQueryOp<T>> Query() {
      return new QueryWrapper<T, QueryConversionExtensions.ListQueryOp<T>>(
                   new QueryConversionExtensions.ListQueryOp<T>(source));
    }

    /// <summary>
    /// Synonym for Query(). Returns a Queryable object on the data in this channel.
    /// </summary>
    public QueryWrapper<T, QueryConversionExtensions.ListQueryOp<T>> GetEach() {
      return Query();
    }

    #endregion

    #region Writes

    /// <summary>
    /// Clears the channel and adds the argument value to it.
    /// </summary>
    public void Set(T value) {
      Clear();
      Add(value);
    }

    /// <summary>
    /// Clears the channel and adds the argument values to it.
    /// </summary>
    public void Set(IList<T> values) {
      Clear();
      Add(values);
    }

    /// <summary>
    /// Clears the channel.
    /// </summary>
    public void Clear() {
      source.Clear();
    }

    /// <summary>
    /// Appends the argument value to the channel.
    /// </summary>
    public void Add(T value) {
      source.Add(value);
    }

    /// <summary>
    /// Appends each of the argument values to the channel.
    /// </summary>
    public void Add(IList<T> values) {
      source.AddRange(values);
    }

    /// <summary>
    /// Removes the first match of the argumnet value from the channel.
    /// </summary>
    public void Remove(T value) {
      source.Remove(value);
    }

    /// <summary>
    /// Removes all values that match the argument predicate from the channel.
    /// </summary>
    public void RemoveAll(Predicate<T> predicate) {
      source.RemoveAll(predicate);
    }

    #endregion

  }

}