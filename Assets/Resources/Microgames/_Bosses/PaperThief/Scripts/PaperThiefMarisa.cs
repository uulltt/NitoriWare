﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaperThiefMarisa : MonoBehaviour
{
    public static PaperThiefMarisa instance;
    public static bool defeated;

#pragma warning disable 0649
    [SerializeField]
    private State state;
    [SerializeField]
    private int maxHealth, moveHealth;
    [SerializeField]
    private float starFireCooldown, hitFlashSpeed, unblackenSpeed, hitFlashColorDrop, defeatSpinFrequency, defeatMoveCenterTime;
    [SerializeField]
    private Vector3 fightPosition;
    [SerializeField]
    private Animator rigAnimator;
    [SerializeField]
    private PaperThiefSpin spin;
    [SerializeField]
    private GameObject starPrefab;
    [SerializeField]
    private Transform starCreationPoint;
    [SerializeField]
    private ParticleSystem broomParticles, defeatedParticles;
    [SerializeField]
    private AudioSource fightSource;
#pragma warning restore 0649

    private List<SpriteRenderer> _spriteRenderers;
    private SineWave _sineWave;
    private bool flashingRed, _blackened;
    private float starFireTimer, defeatSpinTimer, moveCenterSpeed;
    private int health;

    public bool blackened
    {
        get {return _blackened;}
        set 
        {
            if (value)
            {
                for (int i = 0; i < _spriteRenderers.Count; i++)
                {
                    _spriteRenderers[i].color = Color.black;
                }
            }
            _blackened = value;
        }
    }
    
    public enum State
    {
        Cutscene,
        Fight,
        Defeat
    }

    public enum QueueAnimation
    {
        Idle,       //0
        Snap,       //1
        Hurt,       //2
        Zoom,       //3
        Laugh       //4
    }

	void Awake()
	{
        instance = this;
        defeated = false;

        _sineWave = GetComponent<SineWave>();
        _spriteRenderers = new List<SpriteRenderer>();
        addSpriteRenderers(transform);
    }

    void addSpriteRenderers(Transform transform)
    {
        SpriteRenderer sprRenderer = transform.GetComponent<SpriteRenderer>();
        if (sprRenderer != null)
            _spriteRenderers.Add(sprRenderer);
        for (int i = 0; i < transform.childCount; i++)
        {
            addSpriteRenderers(transform.GetChild(i));
        }
    }

    void Start()
    {
        var broomModule = broomParticles.main;
        broomModule.simulationSpace = ParticleSystemSimulationSpace.Custom;
        broomModule.customSimulationSpace = PaperThiefCamera.instance.transform;

        ChangeState(state);
    }

    public void ChangeState(State state)
    {
        switch(state)
        {
            case (State.Cutscene):
                blackened = true;
                break;
            case (State.Fight):
                starFireTimer = starFireCooldown / 2f;
                health = maxHealth;
                transform.localPosition = fightPosition;
                break;
            case (State.Defeat):
                PaperThiefNitori.instance.hasControl = false;
                _sineWave.enabled = false;
                defeatSpinTimer = defeatSpinFrequency / 2f;
                moveCenterSpeed = ((Vector2)transform.localPosition).magnitude / defeatMoveCenterTime;
                defeatedParticles.Play();
                PaperThiefNitori.instance.changeState(PaperThiefNitori.State.Platforming);
                PaperThiefCamera.instance.stopChase();
                fightSource.GetComponent<FadingMusic>().startFade();

                defeated = true;
                break;
            default:
                break;
        }
        rigAnimator.SetInteger("State", (int)state);
        this.state = state;
    }
	
	void Update()
	{
        switch(state)
        {
            case (State.Fight):
                updateFight();
                break;
            case (State.Defeat):
                updateDefeat();
                break;
            default:
                break;
        }

        if (state != State.Cutscene)
        {
            if (flashingRed || _spriteRenderers[0].color.b < 1f)
                updateHitFlash();
        }
        else if (!blackened && _spriteRenderers[0].color.r < 1f)
            updateBlacken();
    }

    void LateUpdate()
    {
        if (PaperThiefNitori.dead)
            stop();
    }

    void updateFight()
    {
        starFireTimer -= Time.deltaTime;
        if (starFireTimer <= 0f)
        {
            queueAnimation(QueueAnimation.Snap);
            starFireTimer = starFireCooldown;
        }

        //if (!_sineWave.enabled)
        //    transform.localPosition = fightPosition;
    }

    void updateDefeat()
    {
        defeatSpinTimer -= Time.deltaTime;
        if (defeatSpinTimer < 0f)
        {
            spin.facingRight = !spin.facingRight;
            defeatSpinTimer = defeatSpinFrequency;
        }

        if (transform.moveTowardsLocal(Vector2.zero, moveCenterSpeed))
        {
            spin.facingRight = false;
            defeatedParticles.Stop();
            enabled = false;
            PaperThiefController.instance.endFight();
        }
    }

    void updateHitFlash()
    {
        Color color = _spriteRenderers[0].color;
        float currentB = color.b, diff = hitFlashSpeed * Time.deltaTime;
        if (flashingRed)
        {
            if (currentB - diff <= hitFlashColorDrop)
            {
                color.g = color.b = hitFlashColorDrop;
                flashingRed = false;
            }
            else
            {
                color.g = color.b = currentB - diff;
            }

        }
        else
        {
            if (currentB + diff >= 1f)
            {
                color.g = color.b = 1f;
            }
            else
            {
                color.g = color.b = currentB + diff;
            }
        }
        for (int i = 0; i < _spriteRenderers.Count; i++)
        {
            _spriteRenderers[i].color = color;
        }
    }
    
    void updateBlacken()
    {
        Color color = _spriteRenderers[0].color;
        float diff = unblackenSpeed * Time.deltaTime;
        if (color.r + diff >= 1f)
            color = Color.white;
        else
            color.r = color.g = color.b = color.r + diff;
        for (int i = 0; i < _spriteRenderers.Count; i++)
        {
            _spriteRenderers[i].color = color;
        }
    }

    public void createStar()
    {
        GameObject newStar = GameObject.Instantiate(starPrefab, starCreationPoint.position, Quaternion.identity);
        newStar.transform.parent = transform.parent;
        PaperThiefStar newStarComponent = newStar.GetComponent<PaperThiefStar>();
        if (_sineWave.enabled)
        {
            if (transform.localPosition.y < 1.5f)
                newStarComponent.forceAngleDirection = -1f;
            else if (transform.localPosition.x < -.5f)
                newStarComponent.forceAngleDirection = 1f;
        }
        //newStarComponent.forceAngleDirection = _sineWave.enabled ? (transform.position.y > 0f ? -1f : 1f) : 0f;
    }

    public void queueAnimation(QueueAnimation animation)
	{
		rigAnimator.SetInteger("QueuedAnimation", (int)animation);
    }

    void stop()
    {
        rigAnimator.enabled = false;
        var broomModule = broomParticles.main;
        broomModule.simulationSpeed = 0f;
        _sineWave.enabled = false;
        enabled = false;
    }

    public void snapToFightPosition()
    {
        transform.localPosition = fightPosition;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (state == State.Fight && !defeated && !PaperThiefNitori.dead && other.name.Contains("Shot"))
        {
            other.GetComponent<PaperThiefShot>().kill();

            queueAnimation(QueueAnimation.Hurt);
            flashingRed = true;
            health--;
            if (health == moveHealth)
            {
                _sineWave.enabled = true;
                _sineWave.resetCycle();
            }
            else if (health <= 0)
                ChangeState(State.Defeat);
        }
    }

    public void setFacingRight(bool facingRight)
    {
        spin.facingRight = facingRight;
    }

    public bool isFacingRight()
    {
        return spin.facingRight;
    }
}
