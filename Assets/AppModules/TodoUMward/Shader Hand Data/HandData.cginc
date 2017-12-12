﻿// HandData.cginc

// ==============
// Fingertip Data
// ==============

uniform float4 _Leap_LH_Fingertips[5];
uniform float4 _Leap_RH_Fingertips[5];

// ================
// Useful Functions
// ================

#define IDENTITY_MATRIX4x4 { 1.0, 0.0, 0.0, 0.0, \
                             0.0, 1.0, 0.0, 0.0, \
                             0.0, 0.0, 1.0, 0.0, \
                             0.0, 0.0, 0.0, 1.0 }

float4x4 Leap_HandData_Preprocess_Matrix = IDENTITY_MATRIX4x4;

// --------
// Distance
// --------

float Leap_Map(float input, float valueMin, float valueMax, float resultMin, float resultMax) {
  if (valueMin == valueMax) return resultMin;
  return lerp(resultMin, resultMax, saturate((input - valueMin) / (valueMax - valueMin)));
}

float Leap_Dist(float4 a, float4 b) {
  float3 ab = b - a;
  return sqrt(ab.x * ab.x + ab.y * ab.y + ab.z * ab.z);
}

float Leap_SqrDist(float3 a, float3 b) {
  float3 ab = b - a;
  return ab.x * ab.x + ab.y * ab.y + ab.z * ab.z;
}

float Leap_DistanceToFingertips(float4 pos) {
	float dist =     Leap_Dist(pos, mul(Leap_HandData_Preprocess_Matrix, _Leap_LH_Fingertips[0]));
	dist = min(dist, Leap_Dist(pos, mul(Leap_HandData_Preprocess_Matrix, _Leap_LH_Fingertips[1])));
	dist = min(dist, Leap_Dist(pos, mul(Leap_HandData_Preprocess_Matrix, _Leap_LH_Fingertips[2])));
	dist = min(dist, Leap_Dist(pos, mul(Leap_HandData_Preprocess_Matrix, _Leap_LH_Fingertips[3])));
	dist = min(dist, Leap_Dist(pos, mul(Leap_HandData_Preprocess_Matrix, _Leap_LH_Fingertips[4])));
	dist = min(dist, Leap_Dist(pos, mul(Leap_HandData_Preprocess_Matrix, _Leap_RH_Fingertips[0])));
	dist = min(dist, Leap_Dist(pos, mul(Leap_HandData_Preprocess_Matrix, _Leap_RH_Fingertips[1])));
	dist = min(dist, Leap_Dist(pos, mul(Leap_HandData_Preprocess_Matrix, _Leap_RH_Fingertips[2])));
	dist = min(dist, Leap_Dist(pos, mul(Leap_HandData_Preprocess_Matrix, _Leap_RH_Fingertips[3])));
	dist = min(dist, Leap_Dist(pos, mul(Leap_HandData_Preprocess_Matrix, _Leap_RH_Fingertips[4])));
	return dist;
}

float Leap_SqrDistToFingertips(float4 pos) {
	float dist =     Leap_SqrDist(pos, mul(Leap_HandData_Preprocess_Matrix, _Leap_LH_Fingertips[0]));
	dist = min(dist, Leap_SqrDist(pos, mul(Leap_HandData_Preprocess_Matrix, _Leap_LH_Fingertips[1])));
	dist = min(dist, Leap_SqrDist(pos, mul(Leap_HandData_Preprocess_Matrix, _Leap_LH_Fingertips[2])));
	dist = min(dist, Leap_SqrDist(pos, mul(Leap_HandData_Preprocess_Matrix, _Leap_LH_Fingertips[3])));
	dist = min(dist, Leap_SqrDist(pos, mul(Leap_HandData_Preprocess_Matrix, _Leap_LH_Fingertips[4])));
	dist = min(dist, Leap_SqrDist(pos, mul(Leap_HandData_Preprocess_Matrix, _Leap_RH_Fingertips[0])));
	dist = min(dist, Leap_SqrDist(pos, mul(Leap_HandData_Preprocess_Matrix, _Leap_RH_Fingertips[1])));
	dist = min(dist, Leap_SqrDist(pos, mul(Leap_HandData_Preprocess_Matrix, _Leap_RH_Fingertips[2])));
	dist = min(dist, Leap_SqrDist(pos, mul(Leap_HandData_Preprocess_Matrix, _Leap_RH_Fingertips[3])));
	dist = min(dist, Leap_SqrDist(pos, mul(Leap_HandData_Preprocess_Matrix, _Leap_RH_Fingertips[4])));
	return dist;
}

float Leap_SqrDistToFingertips_WithScale(float3 pos, float3 scale) {
	float dist =     Leap_SqrDist(pos * scale, mul(Leap_HandData_Preprocess_Matrix, _Leap_LH_Fingertips[0]) * scale);
	dist = min(dist, Leap_SqrDist(pos * scale, mul(Leap_HandData_Preprocess_Matrix, _Leap_LH_Fingertips[1]) * scale));
	dist = min(dist, Leap_SqrDist(pos * scale, mul(Leap_HandData_Preprocess_Matrix, _Leap_LH_Fingertips[2]) * scale));
	dist = min(dist, Leap_SqrDist(pos * scale, mul(Leap_HandData_Preprocess_Matrix, _Leap_LH_Fingertips[3]) * scale));
	dist = min(dist, Leap_SqrDist(pos * scale, mul(Leap_HandData_Preprocess_Matrix, _Leap_LH_Fingertips[4]) * scale));
	dist = min(dist, Leap_SqrDist(pos * scale, mul(Leap_HandData_Preprocess_Matrix, _Leap_RH_Fingertips[0]) * scale));
	dist = min(dist, Leap_SqrDist(pos * scale, mul(Leap_HandData_Preprocess_Matrix, _Leap_RH_Fingertips[1]) * scale));
	dist = min(dist, Leap_SqrDist(pos * scale, mul(Leap_HandData_Preprocess_Matrix, _Leap_RH_Fingertips[2]) * scale));
	dist = min(dist, Leap_SqrDist(pos * scale, mul(Leap_HandData_Preprocess_Matrix, _Leap_RH_Fingertips[3]) * scale));
	dist = min(dist, Leap_SqrDist(pos * scale, mul(Leap_HandData_Preprocess_Matrix, _Leap_RH_Fingertips[4]) * scale));
	return dist;
}

// ------
// Planes
// ------

//float3 Leap_FingertipsDepthInPlane_AssumeHandsInLocalPlaneSpace() {
//  float3 closest = _Leap_LH_Fingertips[0];
//  for (int i = 1; i < 5; i++) {
//    if (_leap_LH_Fingertips[i].z < )
//  }
//}

// Copied-- plane displacement shader
// TODO: Rename, potentially refactor

// Uses dot product to get the component distance along the normal.
// Also sets the parallel and perpendicular vectors.
float distanceToPlane(float3 normal, float4 point1, float4 point2, out float3 perp, out float3 para) {
  float3 diff = (point2 - point1).xyz;
  float normalDotDiff = dot( normal , diff );
  perp = normal * normalDotDiff;
  para = diff - perp;
  return normalDotDiff;
}

// Calculates the displacement of a point by a finger using:
//   - The signed distance (negative is past plane) of the finger relative to the surface of the plane
//   - The "finger influence" of the finger, which is a function of the distance between the point 
//      and the finger along the plane.
//
// The baseDisplacement argument sets the maxiumum distance from the plane that will be reported, allowing
// for the smooth falloff of finger influence.
float getFingerDisplacement( float3 normal, float4 position , float4 fingerPosition, float baseDisplacement, float distanceCutoff) {
  float maxDisplacement = baseDisplacement; // What is is max distance forward we'll report
  float3 para = float3(0.,0.,0.); // Vector pointing parallell to the plane
  float3 perp = float3(0.,0.,0.); // Vector pointing along the normal of the plane
  float d = distanceToPlane( normal , position , fingerPosition , perp , para ); // How far is the finger tip from the plane.
  float diff = min(0, d - maxDisplacement);
  float len = length( para ); // How far is the point from the tip, along the plane
  float fingerInfluence = 0.0;
  if( len <= distanceCutoff) {
    fingerInfluence = pow((1 - (len / distanceCutoff)), 3.);
  }
 
  float displacement = maxDisplacement + (diff * fingerInfluence);
  return displacement;
}

// Return of the greatest displacement caused to the point from a finger. 
float getMinDisplacement(float3 normal, float4 pos, float baseDisplacement, float distanceCutoff) {
  float zDisplacement = baseDisplacement; // 100 units seems a reasonable maxiumum distance.
 
  zDisplacement = min(zDisplacement, getFingerDisplacement( normal, pos, mul(Leap_HandData_Preprocess_Matrix, _Leap_LH_Fingertips[0]), baseDisplacement, distanceCutoff ));
  zDisplacement = min(zDisplacement, getFingerDisplacement( normal, pos, mul(Leap_HandData_Preprocess_Matrix, _Leap_LH_Fingertips[1]), baseDisplacement, distanceCutoff ));
  zDisplacement = min(zDisplacement, getFingerDisplacement( normal, pos, mul(Leap_HandData_Preprocess_Matrix, _Leap_LH_Fingertips[2]), baseDisplacement, distanceCutoff ));
  zDisplacement = min(zDisplacement, getFingerDisplacement( normal, pos, mul(Leap_HandData_Preprocess_Matrix, _Leap_LH_Fingertips[3]), baseDisplacement, distanceCutoff ));
  zDisplacement = min(zDisplacement, getFingerDisplacement( normal, pos, mul(Leap_HandData_Preprocess_Matrix, _Leap_LH_Fingertips[4]), baseDisplacement, distanceCutoff ));
  zDisplacement = min(zDisplacement, getFingerDisplacement( normal, pos, mul(Leap_HandData_Preprocess_Matrix, _Leap_RH_Fingertips[0]), baseDisplacement, distanceCutoff ));
  zDisplacement = min(zDisplacement, getFingerDisplacement( normal, pos, mul(Leap_HandData_Preprocess_Matrix, _Leap_RH_Fingertips[1]), baseDisplacement, distanceCutoff ));
  zDisplacement = min(zDisplacement, getFingerDisplacement( normal, pos, mul(Leap_HandData_Preprocess_Matrix, _Leap_RH_Fingertips[2]), baseDisplacement, distanceCutoff ));
  zDisplacement = min(zDisplacement, getFingerDisplacement( normal, pos, mul(Leap_HandData_Preprocess_Matrix, _Leap_RH_Fingertips[3]), baseDisplacement, distanceCutoff ));
  zDisplacement = min(zDisplacement, getFingerDisplacement( normal, pos, mul(Leap_HandData_Preprocess_Matrix, _Leap_RH_Fingertips[4]), baseDisplacement, distanceCutoff ));
  return zDisplacement;
}

// Get the displacement past the plane (using 0 as base displacement)
float getDisplacement( float3 normal , float4 pos ){
  float pushDist = min(0,getMinDisplacement(normal, pos, 0.0, 0.18));
  float toReturn = clamp(pushDist, -.06, 0. );
  return toReturn;
}

float _normalDisplacementMagnitude = 100.0;
