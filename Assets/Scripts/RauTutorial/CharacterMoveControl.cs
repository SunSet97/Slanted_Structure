using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayMethod
{
    Plt,
    Line,
    Qrt
}

public class CharacterMoveControl : MonoBehaviour
{
    private CharacterManager _cm;
    
    void Start()
    {
        _cm = GetComponent<CharacterManager>();
    }

    private void FixedUpdate()
    {
        if(_cm.isControlled)
            Movement(PlayMethod.Plt);
    }

    #region MovementMethod

    private void Movement(PlayMethod pMethod)
    {
        _cm.camRotation = Quaternion.Euler(0, _cm.cam.transform.eulerAngles.y, 0);
        _cm.moveSpeed = Mathf.Sqrt(_cm.moveHorDir.x * _cm.moveHorDir.x + _cm.moveHorDir.z * _cm.moveHorDir.z);
        _cm.unitVector = Vector3.zero;                                                          //이동 방향 기준 단위 벡터
        _cm.normalizing = 0;
        
        calculJoystick(pMethod);
        checkSpeed();
        checkJump(pMethod);
        calculResistance();
        calculGravity();
        finalMove();
    }

    public void calculJoystick(PlayMethod pMethod)
    {
        switch (pMethod)
        {
            case PlayMethod.Plt:
                if (_cm.joyStick.Horizontal == 0)
                    _cm.unitVector = _cm.moveHorDir.normalized;
                else
                    _cm.unitVector = _cm.camRotation * (Vector3.right * _cm.joyStick.Horizontal).normalized;
                _cm.normalizing = Mathf.Abs(_cm.joyStick.Horizontal);
                break;
            case PlayMethod.Line:
                if (_cm.joyStick.Horizontal == 0 && _cm.joyStick.Vertical == 0)
                    _cm.unitVector = _cm.moveHorDir.normalized; // 현재 움직이는 방향이 마지막으로 입력된 정방향
                else
                    _cm.unitVector = (FindObjectOfType<Waypoint>().moveTo * _cm.joyStick.Horizontal).normalized;
                _cm.normalizing = Mathf.Abs(_cm.joyStick.Horizontal);
                break;
            case PlayMethod.Qrt:
                var _xz = new Vector3(_cm.joyStick.Horizontal, 0, _cm.joyStick.Vertical);
                
                if (_cm.joyStick.Horizontal == 0 && _cm.joyStick.Vertical == 0)
                    _cm.unitVector = _cm.moveHorDir.normalized; // 현재 움직이는 방향이 마지막으로 입력된 정방향
                else
                    _cm.unitVector = _cm.camRotation * _xz.normalized;
                _cm.normalizing = _xz.sqrMagnitude;
                break;
        }
    }

    private void checkSpeed()
    {
        if (_cm.moveSpeed <= _cm.maxMoveSpeed)
        {
            if(!_cm.isSelected || _cm.isDie) _cm.normalizing = 0.0f;
            if (!_cm.ctrl.isGrounded) _cm.normalizing *= 0.2f;
            _cm.moveHorDir += _cm.unitVector * _cm.normalizing * _cm.moveAcceleration * Time.deltaTime;
        }
    }

    public void checkJump(PlayMethod pMethod)
    {
        if (_cm.isSelected && _cm.joyStick.Vertical > 0.5f && _cm.ctrl.isGrounded && pMethod != PlayMethod.Qrt)
        {
            _cm.isJump = true;
        }

        if (_cm.isSelected && !_cm.isDie && _cm.isJump)
        {
            _cm.moveVerDir.y += _cm.jumpForce;
            _cm.isJump = false;
        }
    }
    
    public void calculResistance()
    {
        float resistanceValue = getResistanceValue();

        if (_cm.moveSpeed >= 0.05f)
        {
            if ((_cm.moveHorDir.normalized - _cm.unitVector).sqrMagnitude < 0.8f)
                _cm.moveHorDir -= _cm.unitVector * resistanceValue * Time.deltaTime;
            else
                _cm.moveHorDir += _cm.unitVector * resistanceValue * Time.deltaTime;
        }
    }
    
    public void calculGravity()
    {
        if (!_cm.ctrl.isGrounded || _cm.isDie)
            _cm.moveVerDir.y += Physics.gravity.y * _cm.gravityScale * Time.deltaTime;
    }

    public void finalMove()
    {
        if (DataController.instance_DataController.isMapChanged == false)
            _cm.ctrl.Move((_cm.moveHorDir + _cm.moveVerDir) * Time.deltaTime);
    }

    public float getResistanceValue()
    {
        return _cm.ctrl.isGrounded ? _cm.frictionalForce : _cm.airResistance;
    }
    
    #endregion
}
