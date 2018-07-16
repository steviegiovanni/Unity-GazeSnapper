using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;

public class Ruler : MonoBehaviour, IInputClickHandler {
    private float _distance;
    public float Distance {
        get { return _distance; }
        set { _distance = value; }
    }

    [SerializeField]
    private TextMesh _text;

    enum MeasurerState { Idle, First, Second}

    [SerializeField]
    private MeasurerState _state = MeasurerState.Idle;

    [SerializeField]
    private Snapper _snapper;

    private LineRenderer _lineRenderer;

    private Vector3 p1;
    private Vector3 p2;

	// Use this for initialization
	void Start () {
        InputManager.Instance.AddGlobalListener(this.gameObject);
        _state = MeasurerState.Idle;
        _lineRenderer = GetComponent<LineRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            if (_snapper == null) return;

            if (!_snapper.Hit)
            {
                _state = MeasurerState.Idle;
            }
            else
            { 
                switch (_state)
                {
                    case MeasurerState.Idle:
                        {
                            p1 = _snapper.ReturnValue;
                            _state = MeasurerState.First;
                        }
                        break;
                    case MeasurerState.First:
                        {
                            p2 = _snapper.ReturnValue;
                            _state = MeasurerState.Second;
                        }
                        break;
                    case MeasurerState.Second:
                        {
                            _state = MeasurerState.Idle;
                        }
                        break;
                }
            }
        }

        if (_lineRenderer == null) return;

        switch (_state)
        {
            case MeasurerState.Idle:
                {
                    _lineRenderer.enabled = false;
                    if (_text != null)
                        _text.gameObject.SetActive(false);
                }
                break;
            case MeasurerState.First:
                {
                    _lineRenderer.enabled = false;
                    if (_text != null)
                        _text.gameObject.SetActive(false);
                }
                break;
            case MeasurerState.Second:
                {
                    _lineRenderer.enabled = true;

                    Distance = Vector3.Magnitude(p2-p1);
                    if (_text != null)
                    {
                        _text.gameObject.SetActive(true);
                        _text.text = Distance.ToString();
                    }

                    _lineRenderer.SetPosition(0, p1);
                    _lineRenderer.SetPosition(1, p2);
                }
                break;
        }
	}

    void IInputClickHandler.OnInputClicked(InputClickedEventData eventData)
    {
        if (_snapper == null) return;

        if (!_snapper.Hit){
            //_state = MeasurerState.Idle;
            return;
        }

        switch (_state)
        {
            case MeasurerState.Idle:
                {
                    p1 = _snapper.ReturnValue;
                    _state = MeasurerState.First;
                }break;
            case MeasurerState.First:
                {
                    p2 = _snapper.ReturnValue;
                    _state = MeasurerState.Second;
                }
                break;
            case MeasurerState.Second:
                {
                    _state = MeasurerState.Idle;
                }
                break;
        }
    }
}
