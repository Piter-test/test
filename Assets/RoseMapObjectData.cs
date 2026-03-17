using UnityEngine;
using System.Collections.Generic;
//using Revise.Files;

[System.Serializable]
public class RoseMapObjectData : ScriptableObject {
	[System.Serializable]
	public class SubObject
	{
		public Mesh mesh;
		public Material material;
		public AnimationClip animation;
		public Vector3 position;
		public Quaternion rotation;
		public Vector3 scale;
		public int parent;
		public int colMode;
	}
	[System.Serializable]
	public class EffectData
	{
		public Vector3 position;
		public Quaternion rotation;
		public Vector3 scale;
		public int parent;
		public string effectPath;
        public Material material;

        public float lifetimeMin, lifetimeMax;
        public float emitRateMin, emitRateMax;
        public float TextureWidth, TextureHeight;
        public Vector3 gravityMin, gravityMax;
        public Vector3 spawnDirMin, spawnDirMax;
        public Vector3 emitRadiusMin, emitRadiusMax;
        public int particleCount;
        public int loopCount;
        public int alignment; // AlignmentType

        // Opcjonalnie grawitacja, jeśli będziesz jej używać:
        //public Vector3 gravityMin;
        //public Vector3 gravityMax;
    }

	public List<SubObject> subObjects = new List<SubObject>();
    public List<EffectData> effects = new List<EffectData>();

}
