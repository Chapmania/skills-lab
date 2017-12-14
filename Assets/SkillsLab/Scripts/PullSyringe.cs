﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRTK;

public class PullSyringe : MonoBehaviour {

    private VRTK_InteractableObject interactScript;
    protected Transform snapDrop;
    protected Transform insideSyringe;
    protected Transform fillWater;
    public float speed = 0.1f;
    public float maxMove = 0.1f;
    public float hapticInterval = 0.02f;
    public float hapticIntervalError = 0.005f;
    public float syringeValue = 10; //10ml
    protected Vector3 beginPosition;
    protected Vector3 beginPositionWater;
    protected bool isPulling;
    protected bool isPushing;
    protected Text lcdText;
    protected Transform lcdCanvas;
    protected Transform leftController;
    protected Transform rightController;
    protected VRTK_ControllerEvents leftEvents;
    protected VRTK_ControllerEvents rightEvents;
    //protected bool toggle;

    const string NEEDLELAYER = "needle";
    const string INSIDESYRINGE = "inside";
    const string FILLWATER = "fillWater";
    const string VRTKSCRIPT = "vrtk_scripts";
    const string LCONTR = "LeftController";
    const string RCONTR = "RightController";

    void Start()
    {
        isPulling = false;
        //toggle = false;
        GameObject vrtkScripts = GameObject.FindGameObjectWithTag(VRTKSCRIPT);
        leftController = vrtkScripts.transform.Find(LCONTR);
        rightController = vrtkScripts.transform.Find(RCONTR);
        leftEvents = leftController.GetComponent<VRTK_ControllerEvents>();
        rightEvents = rightController.GetComponent<VRTK_ControllerEvents>();

        snapDrop = this.GetComponentInChildren<VRTK_SnapDropZone>().transform;
        lcdText = this.GetComponentInChildren<Text>();
        lcdCanvas = this.GetComponentInChildren<Canvas>().transform;
        insideSyringe = this.transform.Find(INSIDESYRINGE);
        fillWater = this.transform.Find(FILLWATER);
        beginPosition = insideSyringe.localPosition;
        beginPositionWater = fillWater.localPosition;
        interactScript = GetComponent<VRTK_InteractableObject>();

        lcdCanvas.gameObject.SetActive(false);

        interactScript.InteractableObjectUsed += new InteractableObjectEventHandler(ObjectUsed);
        interactScript.InteractableObjectUnused += new InteractableObjectEventHandler(ObjectUnused);
        interactScript.InteractableObjectGrabbed += new InteractableObjectEventHandler(ObjectGrabbed);
        interactScript.InteractableObjectUngrabbed += new InteractableObjectEventHandler(ObjectUngrabbed);


        leftEvents.TouchpadPressed += new ControllerInteractionEventHandler(LeftTouchpadPressed);
        leftEvents.TouchpadReleased += new ControllerInteractionEventHandler(LeftTouchpadReleased);
        rightEvents.TouchpadPressed += new ControllerInteractionEventHandler(RightTouchpadPressed);
        rightEvents.TouchpadReleased += new ControllerInteractionEventHandler(RightTouchpadReleased);

        //interactScript.InteractableObjectTouched += new InteractableObjectEventHandler(ObjectTouched);
        //interactScript.InteractableObjectUntouched += new InteractableObjectEventHandler(ObjectUntouched);
    }

    protected void ObjectGrabbed(object sender, InteractableObjectEventArgs e)
    {
        if (HasNeedle())
        {
            if (leftController.GetComponent<VRTK_InteractGrab>().GetGrabbedObject() == this.gameObject)
            {
                leftController.GetComponent<VRTK_Pointer>().enabled = false;
            }
            else
            {
                rightController.GetComponent<VRTK_Pointer>().enabled = false;
            }
        }
        else
        {
            rightController.GetComponent<VRTK_Pointer>().enabled = true;
            leftController.GetComponent<VRTK_Pointer>().enabled = true;
        }
    }

    protected void ObjectUngrabbed(object sender, InteractableObjectEventArgs e)
    {
        rightController.GetComponent<VRTK_Pointer>().enabled = true;
        leftController.GetComponent<VRTK_Pointer>().enabled = true;
    }

    protected void LeftTouchpadPressed(object sender, ControllerInteractionEventArgs e)
    {
        if (leftController.GetComponent<VRTK_InteractGrab>().GetGrabbedObject() == this.gameObject)
        {
            ObjectTouchPad();
        }
    }
    protected void LeftTouchpadReleased(object sender, ControllerInteractionEventArgs e)
    {
        if (leftController.GetComponent<VRTK_InteractGrab>().GetGrabbedObject() == this.gameObject)
        {
            ObjectUntouchPad();
        }
    }
    protected void RightTouchpadPressed(object sender, ControllerInteractionEventArgs e)
    {
        if (rightController.GetComponent<VRTK_InteractGrab>().GetGrabbedObject() == this.gameObject)
        {
            ObjectTouchPad();
        }
    }
    protected void RightTouchpadReleased(object sender, ControllerInteractionEventArgs e)
    {
        if (rightController.GetComponent<VRTK_InteractGrab>().GetGrabbedObject() == this.gameObject)
        {
            ObjectUntouchPad();
        }
    }

    public void ObjectTouchPad()
    {
        if (HasNeedle() && !isPulling && !isPulling)
        {
            isPushing = true;
            StartCoroutine(Pushing());
        }       
    }

    public void ObjectUntouchPad()
    {
        isPushing = false;
    }


    private void ObjectUsed(object sender, InteractableObjectEventArgs e)
    {
        if (HasNeedle() && !isPulling && !isPulling)
        {
            isPulling = true;
            StartCoroutine(Pulling());
        }
    }

    private void ObjectUnused(object sender, InteractableObjectEventArgs e)
    {
        isPulling = false;
    }

    protected void ResizeWater(float distance)
    {
        fillWater.localPosition = new Vector3(beginPositionWater.x, beginPositionWater.y + (distance / 2), beginPositionWater.z);
        fillWater.localScale = new Vector3(fillWater.localScale.x, distance / 2, fillWater.localScale.z);
        lcdCanvas.gameObject.SetActive(true);
        //lcdText.text = ((distance / maxMove) * syringeValue).ToString("F2") + " ml";
        lcdText.text = ((Mathf.Round((distance / maxMove) * syringeValue*2))/2.0f).ToString("F2") + " ml";
    }

    protected bool HasNeedle()
    {
        bool returnValue = false;
        foreach (Transform child in snapDrop)
        {
            if (LayerMask.LayerToName(child.gameObject.layer) == NEEDLELAYER)
            {
                returnValue = true;
            }
        }
        return returnValue;
    }

    IEnumerator Pulling()
    {
        while (!isPushing && isPulling && (insideSyringe.localPosition.y - beginPosition.y) < maxMove) 
        {
            yield return new WaitForEndOfFrame();

            float distance = (insideSyringe.localPosition.y - beginPosition.y);
            ResizeWater(distance);
            if (distance % hapticInterval < hapticIntervalError)
            {
                //!! CHANGE HAND TO CURREN THAND GRABBING
                VRTK_ControllerHaptics.TriggerHapticPulse(VRTK_ControllerReference.GetControllerReference(SDK_BaseController.ControllerHand.Right), 0.5f, 0.2f, 0.5f);
            }

            insideSyringe.localPosition += (Vector3.up * Time.deltaTime * speed);
        }
        isPulling = false;
    }

    IEnumerator Pushing()
    {
        while (!isPulling && isPushing && (insideSyringe.localPosition.y - beginPosition.y) > 0)
        {
            yield return new WaitForEndOfFrame();

            float distance = (insideSyringe.localPosition.y - beginPosition.y);
            ResizeWater(distance);

            if (distance%hapticInterval < hapticIntervalError)
            {
                //!! CHANGE HAND TO CURREN THAND GRABBING
                VRTK_ControllerHaptics.TriggerHapticPulse(VRTK_ControllerReference.GetControllerReference(SDK_BaseController.ControllerHand.Right), 0.5f, 0.2f, 0.5f);
            }
            insideSyringe.localPosition -= (Vector3.up * Time.deltaTime * speed);
            
        }
        isPushing = false;
    }
}
