﻿using UnityEngine;
using System.Collections;
using Leap.Unity.Attributes;


namespace Leap.Unity.LeapPaint_v3 {

  public class FilterPositionMovingAverage : IBufferFilter<StrokePoint> {

    private const int NEIGHBORHOOD = 4;

    public int GetMinimumBufferSize() {
      return NEIGHBORHOOD * 2;
    }

    public void Process(RingBuffer<StrokePoint> data, RingBuffer<int> indices) {
      for (int i = Mathf.Min(data.Length - 1, NEIGHBORHOOD); i >= 0; i--) {
        StrokePoint point = data.GetFromEnd(i);
        //point.position = Vector3.Lerp(point.position, CalcNeighborAverage(i, NEIGHBORHOOD, data), 1F / (data.Size - NEIGHBORHOOD));
        point.position = CalcNeighborAverage(i, NEIGHBORHOOD, data);
        data.SetLatest(point);
      }
    }

    private Vector3 CalcNeighborAverage(int index, int R, RingBuffer<StrokePoint> data) {
      Vector3 neighborSum = data.GetFromEnd(index).position;
      int numPointsInRadius = 1;
      while (index + R > data.Length - 1 || index - R < 0) R -= 1;
      for (int r = 1; r <= R; r++) {
        neighborSum += data.GetFromEnd((index - r)).position;
        neighborSum += data.GetFromEnd((index + r)).position;
        numPointsInRadius += 2;
      }
      return neighborSum / numPointsInRadius;
    }

    public void Reset() {
      return;
    }

  }


}