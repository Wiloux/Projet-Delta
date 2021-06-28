using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ToolsBoxEngine;

public class AngerParticles : MonoBehaviour {
    [Serializable]
    private struct Position {
        public Vector3 position;
        public float rotation;
        public float size;
    }

    [Header("Particles")]
    [SerializeField] private Transform particlesParent;
    [SerializeField] private ParticleSystem particle;
    [SerializeField] private Position[] positions;
    private List<ParticleSystem>[] instantied;

    [Header("Anger animation")]
    [SerializeField] private Animator radiantAnimator = null;

    private void Start() {
        instantied = new List<ParticleSystem>[2];
        for (int i = 0; i < instantied.Length; i++) {
            instantied[i] = new List<ParticleSystem>();
        }
    }

    public ParticleSystem Create(Vector3 position, float rotation, float size) {
        ParticleSystem newParticle = Instantiate(particle, particlesParent);
        newParticle.transform.localPosition = position;
        //newParticle.transform.rotation = rotation;
        ParticleSystem.MainModule ps = newParticle.main;
        ps.startRotation = rotation;
        ps.startSize = size;
        newParticle.Play();
        return newParticle;
    }

    public void Create(int side) {
        if (side < 1) { side = 0; }
        else { side = 1; }

        int actualNumber = instantied[side].Count;
        if (actualNumber >= positions.Length) { return; }
        Vector3 pos = positions[actualNumber].position;
        pos.x *= side * 2 - 1;
        ParticleSystem lastPS = Create(pos, positions[actualNumber].rotation, positions[actualNumber].size);
        instantied[side].Add(lastPS);
    }

    public void Unstack(int side) {
        if (side < 1) { side = 0; }
        else { side = 1; }

        if (instantied[side].Count == 0) { return; }

        ParticleSystem particles = instantied[side][instantied[side].Count - 1];

        Destroy(particles.gameObject);
        instantied[side].Remove(particles);
    }

    public void ClearParticles() {
        ClearParticles(-1);
        ClearParticles(1);
    }

    public void ClearParticles(int side) {
        if (side < 1) { side = 0; }
        else { side = 1; }

        for (int i = 0; i < instantied[side].Count; i++) {
            Destroy(instantied[side][i].gameObject);
        }
        instantied[side].Clear();
    }

    public void PlayParticles() {
        PlayParticles(-1);
        PlayParticles(1);
    }

    public void PlayParticles(int side) {
        if (side < 1) { side = 0; }
        else { side = 1; }

        for (int j = 0; j < instantied[side].Count; j++)
            instantied[side][j].Play();
    }

    public void PauseParticles() {
        PauseParticles(-1);
        PauseParticles(1);
    }

    public void PauseParticles(int side) {
        if (side < 1) { side = 0; }
        else { side = 1; }

        for (int j = 0; j < instantied[side].Count; j++)
            instantied[side][j].Pause();
    }

    public void StopParticles() {
        StopParticles(-1);
        StopParticles(1);
    }

    public void StopParticles(int side) {
        if (side < 1) { side = 0; } 
        else { side = 1; }

        for (int j = 0; j < instantied[side].Count; j++)
            instantied[side][j].Stop();
    }

    public void PlayRadiant() {
        radiantAnimator.SetTrigger("DoRadiant");
    }

    private void OnDrawGizmos() {
        if (particlesParent == null) { return; }

        Color color = Color.red;
        color.a = 0.5f;
        Gizmos.color = color;

        Matrix4x4 baseMatrix = Gizmos.matrix;

        for (int i = 0; i < positions.Length; i++) {
            Vector3 position = particlesParent.position + particlesParent.rotation * positions[i].position;
            Quaternion rotation = particlesParent.rotation * Quaternion.Euler(Vector3.zero.Override(positions[i].rotation, Axis.Z));
            Matrix4x4 matrix = baseMatrix;
            matrix.SetTRS(position, rotation, Vector3.one);
            Gizmos.matrix = matrix;
            Gizmos.DrawCube(Vector3.zero, Vector3.one * positions[i].size);
        }
    }
}
