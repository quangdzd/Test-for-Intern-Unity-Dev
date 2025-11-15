using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelTime : LevelCondition
{
    private float m_time;

    private GameManager m_mngr;
    private BoardController m_board;

    public override void Setup(float value, Text txt, GameManager mngr  , BoardController boardController)
    {
        base.Setup(value, txt, mngr);

        m_board = boardController;
        m_mngr = mngr;

        m_time = value;
        m_board.OnMoveEvent += OnMove;

        UpdateText();
    }
    private void OnMove()
    {
        if (m_conditionCompleted) return;

        if(m_board.IsEmpty())
        {
            OnConditionComplete();
        }
    }
    private void Update()
    {
        if (m_conditionCompleted) return;

        if (m_mngr.State != GameManager.eStateGame.GAME_STARTED) return;

        m_time -= Time.deltaTime;

        UpdateText();
        if(m_board.IsEmpty())
        {
            OnConditionComplete();
        }
        if (m_time <= -1f)
        {
            OnConditionLose();
        }
    }

    protected override void UpdateText()
    {
        if (m_time < 0f) return;

        m_txt.text = string.Format("TIME:\n{0:00}", m_time);
    }
    protected override void OnDestroy()
    {
        if (m_board != null) m_board.OnMoveEvent -= OnMove;

        base.OnDestroy();
    }
}
