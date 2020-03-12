//----------------------------------------------
// File: RoomWanderer.cs
// Authors: Daghan Demirci, Ömer Akyol,Yunus Emre Yaman
// Copyright © 2014 Inspire13
//----------------------------------------------

using UnityEngine;
using System.Collections;

public class RoomWanderer : MonoBehaviour
{
    #region Fields

    public Camera WanderCam;
    public Transform[] Spots;
    public float WanderSpeed = 1.0f;
    public AnimationCurve AnimCurve;
    public bool AutoStart;

    private Transform mCamTransform;
    private int mSpotIndex;
    private float mLerpTimer;
    private Vector3 mTempPos;
    private Quaternion mTempRot;

    #endregion

    #region Properties

    public bool IsWandering { get; private set; }

    #endregion

    #region UnityMethods

    void Awake()
    {
        mCamTransform = WanderCam.transform;
    }

    void Start()
    {
        IsWandering = false;
        mSpotIndex = 0;
        mLerpTimer = 0.0f;
        mTempPos = Vector3.zero;
        mTempRot = Quaternion.identity;

        if (AutoStart)
            StartWandering();
    }

    void Update()
    {
        if (IsWandering)
            Wander();

        if (Input.GetKeyDown(KeyCode.Q)) 
            StartWandering();
        else if (Input.GetKeyDown(KeyCode.W))
            StopWander();
    }

    #endregion

    #region PublicMethods

    public void StartWandering()
    {
        mSpotIndex = 0;
        mLerpTimer = 0.0f;
        mCamTransform.position = Spots[mSpotIndex].position;
        mCamTransform.rotation = Spots[mSpotIndex].rotation;
        mTempPos = mCamTransform.position;
        mTempRot = mCamTransform.rotation;
        IsWandering = true;

        if (Spots.Length > 1)
            mSpotIndex++;
    }

    public void StopWander()
    {
        IsWandering = false;
    }

    #endregion

    #region PrivateMethods

    void Wander()
    {
        float curve = AnimCurve.Evaluate(mLerpTimer);
        mCamTransform.position = mTempPos + (Spots[mSpotIndex].position - mTempPos) * curve;
        mCamTransform.rotation = Quaternion.Slerp(mTempRot, Spots[mSpotIndex].rotation, curve);

        mLerpTimer += Time.deltaTime * WanderSpeed;

        if (mLerpTimer > 1.0f)
        {
            NextSpot();
        }
    }

    void NextSpot()
    {
        mTempPos = mCamTransform.position;
        mTempRot = mCamTransform.rotation;
        mLerpTimer = 0.0f;

        mSpotIndex++;
        mSpotIndex = mSpotIndex % Spots.Length;
    }

    #endregion
}
