﻿using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;

namespace TressFX
{
	public class TressFXRender : ATressFXRender
	{
		[Header("Triangle Shadows")]
        /// <summary>
        /// The full shadows flag.
        /// If this is checked, shadow is rendered in triangle topology instead of line topology.
        /// </summary>
        public bool fullShadows = true;

        /// <summary>
        /// The hair material.
        /// </summary>
        public Material hairMaterial;

		/// <summary>
		/// The random texture.
		/// </summary>
		private Texture2D randomTexture;

		/// <summary>
		/// Start this instance.
		/// Initializes the hair material and all other resources.
		/// </summary>
		public override void Awake()
		{
			base.Awake();
			if (this.hairMaterial == null)
			{
				Debug.LogError("No hair material assigned to tressfx hair :(");
				this.enabled = false;
				return;
			}

			// Generate random texture
			this.randomTexture = new Texture2D (128, 128);
			for (int x = 0; x < 128; x++)
			{
				for (int y = 0; y < 128; y++)
				{
					float randomValue = Random.value;
					this.randomTexture.SetPixel (x, y, new Color (randomValue, randomValue, randomValue, randomValue));
				}
			}
			this.randomTexture.Apply ();
		}

        protected override void RenderShadows()
        {
            if (!this.fullShadows)
                base.RenderShadows();
        }

        /// <summary>
        /// </summary>
        public void LateUpdate()
		{
			// Set shader buffers
			this.hairMaterial.SetBuffer ("g_HairVertexTangents", this._master.g_HairVertexTangents);
            this.hairMaterial.SetBuffer("g_HairVertexPositions", this._master.g_HairVertexPositions);
            this.hairMaterial.SetBuffer ("g_TriangleIndicesBuffer", this.g_TriangleIndicesBuffer);
			this.hairMaterial.SetBuffer ("g_HairThicknessCoeffs", this._master.g_HairThicknessCoeffs);
			this.hairMaterial.SetBuffer ("g_TexCoords", this._master.g_TexCoords);
			this.hairMaterial.SetInt ("_VerticesPerStrand", this._master.hairData.m_NumOfVerticesPerStrand);

            // Transformation matrices
            SetSimulationTransformCorrection(this.hairMaterial);
            SetSimulationTransformCorrection(this.shadowMaterial);

            // Set random texture
            this.hairMaterial.SetTexture ("_RandomTex", this.randomTexture);
			
			// Update rendering bounds
			Bounds renderingBounds = new Bounds (this.transform.position + this.renderingBounds.center, this.renderingBounds.size);
			
			// Render meshes
			for (int i = 0; i < this.triangleMeshes.Length; i++)
			{
				this.triangleMeshes[i].bounds = renderingBounds;
				#if UNITY_EDITOR
				if (UnityEditor.SceneView.lastActiveSceneView != null && UnityEditor.SceneView.lastActiveSceneView.camera != null)
					Graphics.DrawMesh (this.triangleMeshes [i], Vector3.zero, Quaternion.identity, this.hairMaterial, 8, UnityEditor.SceneView.lastActiveSceneView.camera, 0, new MaterialPropertyBlock(), this.fullShadows);
				#endif
				foreach (Camera cam in Camera.allCameras)
					Graphics.DrawMesh (this.triangleMeshes [i], Vector3.zero, Quaternion.identity, this.hairMaterial, 8, cam, 0, new MaterialPropertyBlock(), this.fullShadows);
			}

            RenderShadows();
		}
	}
}
