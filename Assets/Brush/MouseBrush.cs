using Normal.Realtime;
using System.Collections.Generic;
using UnityEngine;

public class MouseBrush : MonoBehaviour
{
    // Reference to Realtime to use to instantiate brush strokes
    [SerializeField] private Realtime _realtime;

    // Prefab to instantiate when we draw a new brush stroke
    [SerializeField] private GameObject _brushStrokePrefab = null;

    [SerializeField] private float BrushDistance = 1.0f;

    // Used to keep track of the current brush tip position and the actively drawing brush stroke
    private Vector3 _mousePosition;
    private BrushStroke _activeBrushStroke;

    private void Update()
    {
        if (!_realtime.connected)
            return;

        // Figure out if the trigger is pressed or not
        bool brushTrigger = Input.GetMouseButton(0);

        if (brushTrigger)
        {
            if (Input.GetKey(KeyCode.LeftControl))
            {
                // ereasing
                var brushStrokePicked = GetMouseTarget();
                if (brushStrokePicked != null && brushStrokePicked.GetComponent<RealtimeTransform>().isOwnedLocallySelf)
                    Realtime.Destroy(brushStrokePicked);
            }
            else
            {
                // drawing
                if (_activeBrushStroke == null)
                {
                    // Instantiate a copy of the Brush Stroke prefab.
                    GameObject brushStrokeGameObject = Realtime.Instantiate(_brushStrokePrefab.name, ownedByClient: true, useInstance: _realtime);

                    // Grab the BrushStroke component from it
                    _activeBrushStroke = brushStrokeGameObject.GetComponent<BrushStroke>();

                    // Tell the BrushStroke to begin drawing at the current brush position
                    _activeBrushStroke.BeginBrushStrokeWithBrushTipPoint(GetBrushPos(), GetBrushOrientation());
                    _activeBrushStroke.GetComponent<RealtimeTransform>().RequestOwnership();
                }

                // If the trigger is pressed, and we have a brush stroke, move the brush stroke to the new brush tip position
                _activeBrushStroke.MoveBrushTipToPoint(GetBrushPos(), GetBrushOrientation());
            }
        }
        else if (_activeBrushStroke != null)
        {
            // end up drawing when needed
            _activeBrushStroke.EndBrushStrokeWithBrushTipPoint(GetBrushPos(), GetBrushOrientation());
            _activeBrushStroke = null;
        }
    }

    //// Utility

    private Vector3 GetBrushPos()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        return ray.GetPoint(BrushDistance);
    }

    private Quaternion GetBrushOrientation()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        return Quaternion.LookRotation(ray.direction, Vector3.up);
    }

    private static GameObject GetMouseTarget()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, LayerMask.GetMask("BrushStrokeMesh")))
        {
            Transform hitted = hit.transform;
            return hitted.parent.gameObject;
        }

        return null;
    }
}
