using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CalendarView : MonoBehaviour
{
    [Header("Referências da UI")]
    [SerializeField] private CalendarManager calendarManager; // Referência para o manager de dados
    [SerializeField] private TMP_Text monthYearLabel;
    [SerializeField] private RectTransform contentRect;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private MonthView[] monthViews; // Os 3 painéis de mês
    [SerializeField] private DayView dayViewSample; // Um prefab/exemplo para pegar a largura
    
#if UNITY_EDITOR
    [SerializeField, Range(0,1)] float hnp;
    [SerializeField, Range( 1, 31 )] int currentDay = 1;
#endif

    [Header("Debug (Read Only)")]
    [SerializeField] private DayView _debugCenteredDay;
    [SerializeField] private string _debugCenteredDayName;

    [Header("Configurações de Posição")]
    [SerializeField] private int dayOffset = 0;
    [SerializeField] private float spaceOffset = 0;
    [SerializeField] ScrollRect rect;

    [Header("Detector Central")]
    [Tooltip("RectTransform posicionado no centro da viewport para detectar o dia central")]
    [SerializeField] private RectTransform centerDetector;

    [Header("Animação de Scroll")]
    [Tooltip("Velocidade da animação ao ir para o dia atual (pixels por segundo)")]
    [SerializeField] private float scrollSpeed = 20000f;
    [Tooltip("Distância mínima para considerar que chegou no destino")]
    [SerializeField] private float arrivalThreshold = 5f;
    
    // Mapeamento interno dos painéis
    private Dictionary<Transform, MonthView> _monthViewMap = new();
    private Transform _monthViewsParent;

    // Estado do Calendário
    private DateTime _targetDate;
    private float _dayWidth;
    private DayView _currentCenteredDay;
    private bool _isAnimating;
    
    // Flags para controlar a lógica de scroll, igual ao seu script original
    private bool _isExpectingChangeOnRelease;
    private bool _hasMovedForward;
    private bool _hasMovedBackward;

    private void Awake()
    {
        _monthViewsParent = monthViews[0].transform.parent;
        foreach (var view in monthViews)
        {
            _monthViewMap.Add(view.transform, view);
        }
        // scrollRect.onValueChanged.AddListener(OnScroll);
    }

    /// <summary>
    /// Ponto de entrada para configurar e exibir o calendário.
    /// </summary>
    public void OpenCalendar()
    {
        // Reseta a posição do content para evitar problemas ao reabrir
        contentRect.anchoredPosition = Vector2.zero;

        // Reseta a ordem dos painéis de mês para o estado inicial
        for (int i = 0; i < monthViews.Length; i++)
        {
            monthViews[i].transform.SetSiblingIndex(i);
        }

        _dayWidth = ((RectTransform)dayViewSample.transform).sizeDelta.x +
                    _monthViewsParent.GetComponent<HorizontalLayoutGroup>().spacing;

        _targetDate = DateTime.Today;

        UpdateMonthLabel(_targetDate);

        DayView todayCard = null;
        for (int i = 0; i < 3; i++)
        {
            // Pede ao manager os dados e popula cada painel de mês
            // O 'true' no final significa que este é o mês atual (i=1), para buscar o card de hoje
            DayView foundCard = PopulateMonthView(_monthViewsParent.GetChild(i), _targetDate.AddMonths(i - 1), i == 1);
            if (foundCard != null)
            {
                todayCard = foundCard;
            }
        }
        
        // Seleciona o dia de hoje e move o scroll para ele
        if (todayCard != null)
        {
            // Marca o dia de hoje como selecionado em todos os painéis
            ProcessNewDateSelection(todayCard.Date);
            CenterOnDay(todayCard);
        }
    }

    /// <summary>
    /// Centraliza o scroll em um DayView específico usando o detector central.
    /// </summary>
    public void CenterOnDay(DayView dayView)
    {
        StopAllCoroutines();
        StartCoroutine(AnimateScrollToDayView(dayView));
    }

    private IEnumerator AnimateScrollToDayView(DayView targetDay)
    {
        // Bloqueia interação do usuário durante a animação
        _isAnimating = true;
        scrollRect.enabled = false;

        // Força a atualização do layout antes de calcular posições
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
        Canvas.ForceUpdateCanvases();

        // Espera frames para garantir que tudo foi atualizado
        yield return null;
        yield return null;

        if (centerDetector == null || targetDay == null)
        {
            _isAnimating = false;
            scrollRect.enabled = true;
            yield break;
        }

        // Loop de animação - move gradualmente até o dia estar centralizado
        while (true)
        {
            // Pega a posição X do detector (centro da viewport) em world space
            float detectorX = centerDetector.position.x;

            // Pega a posição X do dia alvo em world space
            float dayX = targetDay.transform.position.x;

            // Calcula a diferença
            float diffX = detectorX - dayX;

            // Atualiza qual dia está no centro agora
            UpdateCenteredDay();

            // Se a diferença for menor que o threshold, chegamos!
            if (Mathf.Abs(diffX) <= arrivalThreshold)
            {
                // Ajuste final para centralizar perfeitamente
                contentRect.anchoredPosition = new Vector2(
                    contentRect.anchoredPosition.x + diffX,
                    contentRect.anchoredPosition.y
                );

                _currentCenteredDay = targetDay;
                _debugCenteredDay = targetDay;
                _debugCenteredDayName = targetDay.Date.ToString("dd/MM/yyyy - dddd");

                // Libera interação do usuário
                _isAnimating = false;
                scrollRect.enabled = true;
                yield break;
            }

            // Move gradualmente na direção do dia alvo
            float moveAmount = scrollSpeed * Time.deltaTime;
            float direction = Mathf.Sign(diffX);

            // Não move mais do que a distância restante
            moveAmount = Mathf.Min(moveAmount, Mathf.Abs(diffX));

            contentRect.anchoredPosition = new Vector2(
                contentRect.anchoredPosition.x + (direction * moveAmount),
                contentRect.anchoredPosition.y
            );

            yield return null;
        }
    }

    /// <summary>
    /// Detecta qual DayView está mais próximo do centro (detector).
    /// Chame isso no OnValueChanged do ScrollRect se quiser atualizar em tempo real.
    /// </summary>
    public DayView GetDayAtCenter()
    {
        if (centerDetector == null) return null;

        float detectorX = centerDetector.position.x;
        DayView closestDay = null;
        float closestDistance = float.MaxValue;

        foreach (var monthView in monthViews)
        {
            foreach (var dayView in monthView.GetComponentsInChildren<DayView>(false))
            {
                float distance = Mathf.Abs(dayView.transform.position.x - detectorX);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestDay = dayView;
                }
            }
        }

        return closestDay;
    }

    /// <summary>
    /// Atualiza o dia central atual. Pode ser chamado no OnValueChanged do ScrollRect.
    /// </summary>
    public void UpdateCenteredDay()
    {
        _currentCenteredDay = GetDayAtCenter();

        // Atualiza debug no Inspector
        _debugCenteredDay = _currentCenteredDay;
        _debugCenteredDayName = _currentCenteredDay != null
            ? _currentCenteredDay.Date.ToString("dd/MM/yyyy - dddd")
            : "Nenhum";
    }

    /// <summary>
    /// Retorna o dia que está atualmente no centro.
    /// </summary>
    public DayView CurrentCenteredDay => _currentCenteredDay;

    /// <summary>
    /// Move o scroll para um dia específico do mês atual.
    /// Exatamente a mesma lógica do seu script `UpdateValue` e `DelayedValueUpdate`.
    /// </summary>
    public void GoToDay(int day)
    {
        int daysInMonth = DateTime.DaysInMonth(_targetDate.Year, _targetDate.Month);
        day = Mathf.Clamp(day, 1, daysInMonth);
        StopAllCoroutines();
        StartCoroutine(AnimateScrollToDay(day));
    }

    private IEnumerator AnimateScrollToDay(int day)
    {
        // Força a atualização do layout antes de calcular posições
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
        Canvas.ForceUpdateCanvases();

        // Espera dois frames para garantir que tudo foi atualizado
        yield return null;
        yield return null;

        DateTime prevMonthDate = _targetDate.AddMonths(-1);
        int daysInPrevMonth = DateTime.DaysInMonth(prevMonthDate.Year, prevMonthDate.Month);

        // Calcula a largura da viewport para centralizar
        float viewportWidth = scrollRect.viewport != null
            ? ((RectTransform)scrollRect.viewport).rect.width
            : ((RectTransform)scrollRect.transform).rect.width;

        // Posição do dia no content
        float dayPositionX = (_dayWidth * daysInPrevMonth) + ((day - 1 + dayOffset) * _dayWidth);

        // Centraliza: posição do dia - (metade da viewport - metade do dia)
        float targetX = -(dayPositionX - (viewportWidth / 2f) + (_dayWidth / 2f)) + spaceOffset;

        contentRect.anchoredPosition = new Vector2(targetX, contentRect.anchoredPosition.y);
    }

    #region Lógica de Scroll (Idêntica à sua)

    public void UpdateCalendarMonths( Vector2 val ) {
#if UNITY_EDITOR
        hnp = rect.horizontalNormalizedPosition;
#endif
        if ( !_isExpectingChangeOnRelease ) return;
        if( val.x <= .3f && !_hasMovedBackward ) {
            _isExpectingChangeOnRelease = false;
            _hasMovedBackward = true;
            MoveBackward();
        }
        if( val.x >= .7f && !_hasMovedForward ) {
            _isExpectingChangeOnRelease = false;
            _hasMovedForward = true; 
            MoveForward();
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Ao soltar, verifica a posição e troca o mês se necessário
        if (scrollRect.horizontalNormalizedPosition <= 0.3f && !_hasMovedBackward) MoveBackward();
        if (scrollRect.horizontalNormalizedPosition >= 0.7f && !_hasMovedForward) MoveForward();
        
        // Habilita a troca de mês no próximo scroll (se o usuário não soltar)
        _isExpectingChangeOnRelease = true;
    }
    
    public void DraginDeezNuts() => _isExpectingChangeOnRelease = _hasMovedBackward = _hasMovedForward = false;
    
    public void ReleasedMouse() {
        if ( rect.horizontalNormalizedPosition <= .3f ) MoveBackward();
        if ( rect.horizontalNormalizedPosition >= .7f ) MoveForward();
        _isExpectingChangeOnRelease = true;
    }
    
    #endregion

    #region Manipulação dos Painéis de Mês (Idêntica à sua)

    private void MoveForward()
    {
        Transform firstPanel = _monthViewsParent.GetChild(0);
        float panelWidth = ((RectTransform)firstPanel).sizeDelta.x;
        
        // Move o painel para o final
        firstPanel.SetAsLastSibling();
        
        // Atualiza a data do painel movido
        _targetDate = _targetDate.AddMonths(1);
        PopulateMonthView(firstPanel, _targetDate.AddMonths(1), false);
        
        // Compensa a posição do conteúdo para criar o efeito de scroll infinito
        contentRect.anchoredPosition = new Vector2(contentRect.anchoredPosition.x + panelWidth, contentRect.anchoredPosition.y);
        
        UpdateMonthLabel(_targetDate);
    }

    private void MoveBackward()
    {
        Transform lastPanel = _monthViewsParent.GetChild(_monthViewsParent.childCount - 1);
        float panelWidth = ((RectTransform)lastPanel).sizeDelta.x;

        // Move o painel para o começo
        lastPanel.SetAsFirstSibling();
        
        // Atualiza a data
        _targetDate = _targetDate.AddMonths(-1);
        PopulateMonthView(lastPanel, _targetDate.AddMonths(-1), false);
        
        // Compensa a posição
        contentRect.anchoredPosition = new Vector2(contentRect.anchoredPosition.x - panelWidth, contentRect.anchoredPosition.y);

        UpdateMonthLabel(_targetDate);
    }

    #endregion

    /// <summary>
    /// Função auxiliar para popular um painel de mês e retornar o card de hoje, se encontrado.
    /// </summary>
    private DayView PopulateMonthView(Transform panelTransform, DateTime month, bool isCurrentMonth)
    {
        MonthView monthView = _monthViewMap[panelTransform];
        // Agora, o callback de clique aponta para o nosso novo método "maestro"
        return monthView.Populate(month, isCurrentMonth, calendarManager.HasEventOnDate, ProcessNewDateSelection);
    }
    
    private void ProcessNewDateSelection(DateTime date)
    {
        // 1. Manda o CalendarManager fazer sua parte: carregar e exibir os eventos.
        calendarManager.OnDateSelected(date);

        // 2. Manda todos os painéis de mês atualizarem o estado visual de seus dias.
        foreach (var monthView in monthViews)
        {
            monthView.UpdateSelection(date);
        }
    }

    private void UpdateMonthLabel(DateTime date)
    {
        monthYearLabel.text = $"{date:MMMM, yyyy}".ToUpper();
    }
}