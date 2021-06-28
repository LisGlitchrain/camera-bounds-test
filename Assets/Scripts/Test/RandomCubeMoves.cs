using UnityEngine;

public class RandomCubeMoves : MonoBehaviour
{
    [SerializeField] bool m_local;
    [SerializeField] Vector3 m_actionBox;
    [SerializeField] float m_movePeriod;
    [SerializeField] Vector3 m_rotationAxis;
    [SerializeField] float m_rotationPeriod;

    float m_moveTime = 0;
    float m_rotateTime = 0;
    Vector3 m_defaultPosition;
    Quaternion m_defaultRotation;
    Vector3 m_currentPosition;
    Quaternion m_currentRotation;


    private void OnEnable()
    {
        if (m_local)
        {
            m_defaultPosition = transform.localPosition;
            m_defaultRotation = transform.localRotation;
        }
        else
        {
            m_defaultPosition = transform.position;
            m_defaultRotation = transform.rotation;
        }
    }

    private void Update()
    {
        Vector3 m_actionRadius = 0.5f * m_actionBox;

        if (m_actionBox != Vector3.zero && m_movePeriod > 0)
            m_currentPosition = m_defaultPosition + Mathf.Sin(Mathf.PI * m_moveTime) * m_actionRadius;
        else
            m_currentPosition = m_defaultPosition;

        if (m_rotationAxis != Vector3.zero && m_rotationPeriod > 0)
            m_currentRotation = m_defaultRotation * Quaternion.Euler(0, 360 * m_rotateTime, 0) * Quaternion.FromToRotation(Vector3.up, m_rotationAxis);
        else
            m_currentRotation = m_defaultRotation;

        if (m_local)
        {
            transform.localPosition = m_currentPosition;
            transform.localRotation = m_currentRotation;
        }
        else
        {
            transform.position = m_currentPosition;
            transform.rotation = m_currentRotation;
        }

        UpdateTime();
    }

    private void UpdateTime()
    {
        m_moveTime += Time.deltaTime / m_movePeriod;
        m_rotateTime += Time.deltaTime / m_rotationPeriod;

        if (m_moveTime >= 1)
            m_moveTime = -1;

        if (m_rotateTime >= 1)
            m_rotateTime = -1;
    }
}
