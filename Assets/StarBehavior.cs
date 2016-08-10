﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StarBehavior : InteractObject {

    private Rigidbody rig;

    private ThrowingStar_WandController starWand;

    public float throwingVelocityMultiplier = 1.0f;

    public Vector3 attachAngleOffset;
    public Vector3 attachCenterOffset;

    public float lifeSpan = 5.0f;

    private Vector3 SdeltaPos;
    private Vector3 lastPos;
    private Quaternion SdeltaRot;
    private Quaternion lastRot;

    private float Sangle;
    private Vector3 Saxis;

    // Use this for initialization
    new void Start () {
        base.Start();

        rig = GetComponent<Rigidbody>();

        SdeltaPos = new Vector3();
        SdeltaRot = new Quaternion();

        lastPos = new Vector3();
        lastRot = new Quaternion();
	}
	
	// Update is called once per frame
	new void Update ()
    {
        base.Update();
        if (IsInteracting())
        {   // Attach directly to hand
            //WandController attachedWand = GetAttachedWand();
            //transform.position = attachedWand.transform.position + attachCenterOffset;
            //Vector3 temp = attachedWand.transform.rotation.eulerAngles;
            //temp += attachAngleOffset;
            //transform.rotation = Quaternion.Euler(temp);

            // Track our deltas using last position
            SdeltaPos = transform.position - lastPos;

            SdeltaRot = transform.rotation * Quaternion.Inverse(lastRot);
            SdeltaRot.ToAngleAxis(out Sangle, out Saxis);

            lastPos = transform.position;
            lastRot = transform.rotation;
            //transform.RotateAroundLocal(new Vector3(0.0f, 0.0f, 0.0f), 30.0f);
            //transform.Rotate(new Vector3(30.0f, 0.0f, 0.0f));
            //Debug.Log(transform.eulerAngles);
        } else
        {
            // Lower life until dead
            lifeSpan -= Time.deltaTime;
            if (lifeSpan <= 0.0f)
            {
                Destroy(gameObject);
            }
        }
    }

    void OnCollisionEnter(Collision col)
    {
        Debug.Log("Collision! " + col.ToString());
    }

    public void Init(WandController wand)
    {
        // Grab StarWandCtrllr and add our star
        starWand = wand.GetComponent<ThrowingStar_WandController>();
        starWand.addStar(this);

        Debug.Log("[StarBehavior.Init]");

        wand.pickupObject(this, 4, Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger);
    }

    public override void OnTriggerDown(WandController wand)
    {
        Init(wand);
    }

    public override void BeginInteraction(WandController wand)
    {
        base.BeginInteraction(wand);

        // Override interactionPoint using starOffsets
        if (starWand) {
            Vector3 posOffset;
            Vector3 rotOffset;
            Debug.Log("[StarBeh.BeginInter] Enter");
            starWand.getStarOffsets(this, out posOffset, out rotOffset);
            Debug.Log("[StarBeh.BeginInter] posOffset: " + posOffset);
            Debug.Log("[StarBeh.BeginInter] rotOffset: " + (wand.transform.rotation.eulerAngles + rotOffset));
            // Transform to world coordinates
            //posOffset = wand.transform.TransformPoint(posOffset);

            Debug.Log("IntPt: " + GetInteractionPoint().rotation);
            SetInteractionPointLocal(posOffset, rotOffset);
            Debug.Log("AfterIntPt: " + GetInteractionPoint().rotation);
        } else
        {
            Debug.Log("[StarBehavior.BeginInteraction] StarWand not set!");
        }
        
        if(rig)
        {
            rig.detectCollisions = false;
            rig.isKinematic = true;
        } else
        {   // Try to get it again
            rig = GetComponent<Rigidbody>();
            if (rig)
            {
                rig.detectCollisions = false;
                rig.isKinematic = true;
            }
        }
    }

    public override void EndInteraction(WandController wand)
    {
        if (IsInteracting())
        {   // Update physics to follow attached wand
            WandController attachedWand = GetAttachedWand();
            Rigidbody trigidbody = GetComponent<Rigidbody>();
            float tvelocityFactor = GetVelocityFactor();

            trigidbody.velocity = SdeltaPos * tvelocityFactor * Time.fixedDeltaTime * throwingVelocityMultiplier;
            trigidbody.angularVelocity = (Time.fixedDeltaTime * Sangle * Saxis) * GetRotationFactor() * throwingVelocityMultiplier;

#if DEBUG
            Debug.Log("wandPos: " + attachedWand.transform.position);
            Debug.Log("Pos: " + trigidbody.velocity + " ; delta: " + SdeltaPos + " ; factor: " + tvelocityFactor + " ; time: " + Time.fixedDeltaTime);
            Debug.Log("Rot: " + trigidbody.angularVelocity);
#endif
            #region AutoAim
            // ====================================
            // Auto-aim logic
            //
            // Collect potential targets by filtering through projection axis distance, and collecting positive results (objects in front of velocity projection)
            List<GameObject> potentialTargets = new List<GameObject>();
            Vector3 calcVel;
            Transform objTrans;
            GameObject tmpGO = new GameObject();
            Quaternion lowestDiff;
            GameObject bestTarget = null;
            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("AutoAimTarget"))
            {   // Find any and all potential autoAim targets
                objTrans = obj.GetComponentInParent<Transform>();
                //calcVel = objTrans.position - transform.loo
                tmpGO.transform.position = transform.position;
                tmpGO.transform.LookAt(objTrans);
                Quaternion diff = transform.rotation * Quaternion.Inverse(tmpGO.transform.rotation);
            }
            #endregion
        }
        base.EndInteraction(wand);

        // Remove star from StarWand
        if (starWand)
        {
            starWand.removeStar(this);
            starWand = null;
        }

        // Set to active physics object
        Rigidbody rigidbody = GetComponent<Rigidbody>();
        if (rigidbody)
        {
            rigidbody.detectCollisions = true;
            rigidbody.isKinematic = false;
        }
    }
}