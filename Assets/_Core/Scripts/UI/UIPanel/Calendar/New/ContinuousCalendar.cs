using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ContinuousCalendar : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform content;
    [SerializeField] private TMP_Text monthYearLabel;

    [Header("Prefabs & Layout")]
    [SerializeField] private DayView dayViewPrefab;
    [SerializeField] private float dayWidth = 150f; // Largura do prefab DayView
    [SerializeField] private float daySpacing = 10f; // Espaçamento entre os dias
    [SerializeField] private int bufferDays = 5; // Quantos dias extras renderizar fora da tela

    // Eventos para comunicação com o CalendarManager
    public Action<DateTime> OnDateSelected;
    public Func<DateTime, bool> OnCheckForEvent; // Função para perguntar se uma data tem evento

    private List<DayView> _dayViewsPool = new List<DayView>();
    private DateTime _centerDate;
    private DateTime _leftmostDate;
    private DateTime _rightmostDate;

    private float _halfViewportWidth;
    private int _visibleDaysCount;
    private int _poolSize;
    private bool _isInitialized = false;

    private void Start()
    {
        scrollRect.onValueChanged.AddListener(OnScroll);
    }

    public void Initialize(DateTime startDate)
    {
        // Limpa estado anterior, se houver
        foreach (var view in _dayViewsPool) Destroy(view.gameObject);
        _dayViewsPool.Clear();

        _halfViewportWidth = (transform as RectTransform).rect.width / 2f;
        _visibleDaysCount = Mathf.CeilToInt((_halfViewportWidth * 2) / (dayWidth + daySpacing));
        _poolSize = _visibleDaysCount + bufferDays * 2; // Dias visíveis + buffer de cada lado

        _centerDate = startDate.Date;
        
        // Configura o tamanho do conteúdo para simular um scroll "infinito"
        // Um range de 10 anos (5 para trás, 5 para frente) é mais que suficiente
        int totalDaysInRange = 365 * 10;
        content.sizeDelta = new Vector2(totalDaysInRange * (dayWidth + daySpacing), content.sizeDelta.y);

        // Cria o pool inicial de DayViews
        for (int i = 0; i < _poolSize; i++)
        {
            DayView dayView = Instantiate(dayViewPrefab, content);
            dayView.gameObject.SetActive(true);
            _dayViewsPool.Add(dayView);
        }

        // Posiciona o scroll na data inicial
        float initialContentPosX = -CalculatePositionForDate(startDate);
        content.anchoredPosition = new Vector2(initialContentPosX, content.anchoredPosition.y);
        
        LayoutPoolAroundDate(startDate);
        UpdateMonthLabel(startDate);

        _isInitialized = true;
    }

    private void OnScroll(Vector2 position)
    {
        if (!_isInitialized) return;

        // Calcula qual data está no centro da tela com base na posição do scroll
        float currentX = -content.anchoredPosition.x;
        DateTime newCenterDate = DateFromPosition(currentX);

        if (newCenterDate != _centerDate)
        {
            _centerDate = newCenterDate;
            UpdateMonthLabel(_centerDate);
            RecycleViews();
        }
    }

    private void RecycleViews()
    {
        DateTime visibleStartDate = DateFromPosition(-content.anchoredPosition.x - _halfViewportWidth);
        
        // Verifica se os DayViews precisam ser movidos da direita para a esquerda
        while (visibleStartDate.Date < _leftmostDate.Date)
        {
            // Pega o último item (mais à direita)
            DayView viewToMove = _dayViewsPool.Last();
            _dayViewsPool.RemoveAt(_dayViewsPool.Count - 1);

            // Move para o início da lista e da cena
            _dayViewsPool.Insert(0, viewToMove);
            _leftmostDate = _leftmostDate.AddDays(-1);
            
            SetupDayView(viewToMove, _leftmostDate);
            viewToMove.transform.localPosition = new Vector2(CalculatePositionForDate(_leftmostDate), 0);
        }
        
        // Verifica se os DayViews precisam ser movidos da esquerda para a direita
        DateTime visibleEndDate = DateFromPosition(-content.anchoredPosition.x + _halfViewportWidth);
        while (visibleEndDate.Date > _rightmostDate.Date)
        {
            // Pega o primeiro item (mais à esquerda)
            DayView viewToMove = _dayViewsPool.First();
            _dayViewsPool.RemoveAt(0);
            
            // Move para o final da lista e da cena
            _dayViewsPool.Add(viewToMove);
            _rightmostDate = _rightmostDate.AddDays(1);
            
            SetupDayView(viewToMove, _rightmostDate);
            viewToMove.transform.localPosition = new Vector2(CalculatePositionForDate(_rightmostDate), 0);
        }
    }

    private void LayoutPoolAroundDate(DateTime date)
    {
        int centerIndex = _poolSize / 2;
        _leftmostDate = date.AddDays(-centerIndex);
        _rightmostDate = _leftmostDate.AddDays(_poolSize - 1);

        for (int i = 0; i < _poolSize; i++)
        {
            DateTime currentDate = _leftmostDate.AddDays(i);
            DayView view = _dayViewsPool[i];
            
            SetupDayView(view, currentDate);
            view.transform.localPosition = new Vector2(CalculatePositionForDate(currentDate), 0);
        }
    }

    private void SetupDayView(DayView view, DateTime date)
    {
        bool hasEvent = OnCheckForEvent?.Invoke(date.Date) ?? false;
        view.Setup(date, hasEvent);
        view.OnDateSelected.RemoveAllListeners();
        view.OnDateSelected.AddListener(dateSelected => OnDateSelected?.Invoke(dateSelected));
    }

    private void UpdateMonthLabel(DateTime date)
    {
        string monthName = $"{date:MMMM, yyyy}".ToUpper();
        if (monthYearLabel.text != monthName)
        {
            monthYearLabel.text = monthName;
        }
    }

    // Funções de cálculo
    private float CalculatePositionForDate(DateTime date)
    {
        // Posição relativa a hoje (01/01/0001)
        return (date.Date - DateTime.MinValue.Date).Days * (dayWidth + daySpacing);
    }
    
    private DateTime DateFromPosition(float xPos)
    {
        int dayIndex = Mathf.RoundToInt(xPos / (dayWidth + daySpacing));
        return DateTime.MinValue.Date.AddDays(dayIndex);
    }
}