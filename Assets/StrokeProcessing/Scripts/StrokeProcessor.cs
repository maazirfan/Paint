﻿using UnityEngine;
using System.Collections.Generic;
using System;

public class StrokeProcessor {

  #region Stroke State

  // Stroke processing configuration
  private List<IMemoryFilter<StrokePoint>> _strokeFilters = null;
  private int _maxMemory = 0;

  // Stroke state
  private bool _isBufferingStroke = false;
  private bool _isActualizingStroke = false;
  private RingBuffer<StrokePoint> _strokeBuffer;
  private RingBuffer<int> _strokeIdxBuffer;
  private int _curStrokeIdx = 0;
  private List<StrokePoint> _strokeOutput = null;
  private int _outputBufferEndOffset = 0;

  // Stroke renderers
  private List<IStrokeRenderer> _strokeRenderers = null;

  // Stroke buffer renderers
  private List<IStrokeBufferRenderer> _strokeBufferRenderers = null;

  public StrokeProcessor() {
    _strokeFilters = new List<IMemoryFilter<StrokePoint>>();
    _strokeRenderers = new List<IStrokeRenderer>();
    _strokeBufferRenderers = new List<IStrokeBufferRenderer>();
    _strokeOutput = new List<StrokePoint>();
  }

  public void RegisterStrokeFilter(IMemoryFilter<StrokePoint> strokeFilter) {
    _strokeFilters.Add(strokeFilter);

    int filterMemorySize = strokeFilter.GetMemorySize();
    if (filterMemorySize + 1 > _maxMemory) {
      _maxMemory = filterMemorySize + 1;
    }

    if (_isBufferingStroke) {
      Debug.LogWarning("[StrokeProcessor] Registering stroke filters destroys the current stroke processing queue.");
    }
    _strokeBuffer = new RingBuffer<StrokePoint>(_maxMemory);
    _strokeIdxBuffer = new RingBuffer<int>(_maxMemory);
  }

  public void RegisterStrokeRenderer(IStrokeRenderer strokeRenderer) {
    _strokeRenderers.Add(strokeRenderer);
    if (_isBufferingStroke) {
      Debug.LogError("[StrokeProcessor] Stroke in progress; Newly registered stroke renderers will not render the entire stroke if a stroke is already in progress.");
    }
  }

  public void RegisterPreviewStrokeRenderer(IStrokeBufferRenderer strokeBufferRenderer) {
    _strokeBufferRenderers.Add(strokeBufferRenderer);
    if (_isBufferingStroke) {
      Debug.LogError("[StrokeProcessor] Stroke buffer already active; Newly registered stroke buffer renderers will not render the entire preview stroke if a stroke is already in progress.");
    }
  }

  public void BeginStroke() {
    if (_isBufferingStroke) {
      Debug.LogError("[StrokeProcessor] Stroke in progress; cannot begin new stroke. Call EndStroke() to finalize the current stroke first.");
      return;
    }
    _isBufferingStroke = true;

    _strokeBuffer.Clear();
    _strokeIdxBuffer.Clear();

    for (int i = 0; i < _strokeFilters.Count; i++) {
      _strokeFilters[i].Reset();
    }
    for (int i = 0; i < _strokeBufferRenderers.Count; i++) {
      _strokeBufferRenderers[i].InitializeRenderer();
    }
  }

  public void StartActualizingStroke() {
    if (!_isBufferingStroke) {
      BeginStroke();
    }

    if (_isActualizingStroke) {
      Debug.LogError("[StrokeProcessor] Stroke already actualizing; cannot begin actualizing stroke. Call StopActualizingStroke() first.");
      return;
    }
    _isActualizingStroke = true;
    _strokeOutput = new List<StrokePoint>(); // can't clear -- other objects have references to the old stroke output.
    _outputBufferEndOffset = 0;

    for (int i = 0; i < _strokeRenderers.Count; i++) {
      _strokeRenderers[i].InitializeRenderer();
    }
  }

  public void UpdateStroke(StrokePoint strokePoint) {
    _strokeBuffer.Add(strokePoint);
    _strokeIdxBuffer.Add(_curStrokeIdx++);

    // Apply all filters in order on current stroke buffer.
    for (int i = 0; i < _strokeFilters.Count; i++) {
      _strokeFilters[i].Process(_strokeBuffer, _strokeIdxBuffer);
    }

    if (_isActualizingStroke) {
      // Output points from the buffer to the actualized stroke output.
      int offset = Mathf.Min(_outputBufferEndOffset, _strokeBuffer.Size - 1);
      for (int i = 0; i <= offset; i++) {
        int outputIdx = Mathf.Max(0, _outputBufferEndOffset - (_strokeBuffer.Size - 1)) + i;
        StrokePoint bufferStrokePoint = _strokeBuffer.GetFromEnd(Mathf.Min(_strokeBuffer.Size - 1, _outputBufferEndOffset) - i);
        if (outputIdx > _strokeOutput.Count - 1) {
          _strokeOutput.Add(bufferStrokePoint);
        }
        else {
          _strokeOutput[outputIdx] = bufferStrokePoint;
        }
      }
      _outputBufferEndOffset += 1;

      // Refresh stroke renderers.
      for (int i = 0; i < _strokeRenderers.Count; i++) {
        _strokeRenderers[i].RefreshRenderer(_strokeOutput, _maxMemory);
      }
    }

    // Refresh stroke preview renderers.
    for (int i = 0; i < _strokeBufferRenderers.Count; i++) {
      _strokeBufferRenderers[i].RefreshRenderer(_strokeBuffer);
    }
  }

  public void StopActualizingStroke() {
    _isActualizingStroke = false;

    for (int i = 0; i < _strokeRenderers.Count; i++) {
      _strokeRenderers[i].FinalizeRenderer();
    }
  }

  public void EndStroke() {
    if (_isActualizingStroke) {
      StopActualizingStroke();
    }

    _isBufferingStroke = false;

    for (int i = 0; i < _strokeBufferRenderers.Count; i++) {
      _strokeBufferRenderers[i].StopRenderer();
    }
  }

  #endregion

}
