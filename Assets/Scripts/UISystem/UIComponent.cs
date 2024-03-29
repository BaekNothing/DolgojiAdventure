using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace UIObject
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UIBehaviour))]
    public class UIComponent : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
    {
        public struct UIComponentInitData
        {
            public Type type;
            public IViewData data;
            public Func<IViewData, ComponentStatus, ComponentStatus> evaluator;
            public Func<UIBehaviour, IViewData, ComponentStatus, UIBehaviour> draw;
            public ViewBase parent;

            public UIComponentInitData(Type type, IViewData data, Func<IViewData, ComponentStatus, ComponentStatus> evaluator, Func<UIBehaviour, IViewData, ComponentStatus, UIBehaviour> draw, ViewBase parent)
            {
                if (type != typeof(TextMeshProUGUI) && type != typeof(Text) &&
                    type != typeof(Image) && type != typeof(Button))
                    throw new ArgumentException($"UIComponent.Init: type is not Text, Image or Button");

                if (data == null || evaluator == null || draw == null)
                    throw new ArgumentNullException($"UIComponent.Init: argument is null");

                this.type = type;
                this.data = data;
                this.evaluator = evaluator;
                this.draw = draw;
                this.parent = parent;
            }
        }


        public ComponentStatus Status = ComponentStatus.Enable;
        [SerializeField] UIBehaviour ComponentBody;
        [SerializeField] ComponentAction Action = new();

        IViewData _viewData;
        ViewBase _parent = null;

        Func<IViewData, ComponentStatus, ComponentStatus> Evaluator { get; set; } = (data, status) =>
            throw new NotImplementedException("Evaluator is not implemented");
        Func<UIBehaviour, IViewData, ComponentStatus, UIBehaviour> Draw { get; set; } = (body, data, status) =>
            throw new NotImplementedException($"Draw is not implemented");

        public void Refresh()
        {
            if (!this) return;

            if (ComponentBody == null || _viewData == null || Evaluator == null || Draw == null)
                throw new NullReferenceException($"{this.gameObject.name} UIComponent.Refresh: component is not initialized");

            Status = Evaluator(_viewData, Status);
            Draw(ComponentBody, _viewData, Status);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_parent == null ||
                !_parent.IsTop() ||
                _parent.IsPaused)
                return;

            Utility.Logger.Log($"{_parent?.IsTop()} {gameObject.name} UIComponent.OnPointerClick: {eventData.button} button is clicked");

            Action?.Invoke();
        }

        public void Initialized(UIComponentInitData initData)
        {
            var body = transform.GetComponent(initData.type) as UIBehaviour;

            if (!body)
                throw new NullReferenceException($"{this.gameObject.name} UIComponent.Init: body is null");

            SetBody(body);
            SetParent(initData.parent);
            SetData(initData.data);
            SetEvaluator(initData.evaluator);
            SetDraw(initData.draw);
        }

        public void SetBody(UIBehaviour body)
        {
            Debug.Assert(body != null);
            ComponentBody = body;
        }

        public void SetParent(ViewBase parent)
        {
            Debug.Assert(parent != null);
            _parent = parent;
        }

        public void SetData(IViewData data)
        {
            Debug.Assert(data != null);

            if (_viewData != null && _viewData != data)
                _viewData.RemoveRefreshAction<UIComponent>(Refresh);
            else if (_viewData == data)
                return;

            data.AddRefreshAction<UIComponent>(Refresh);
            _viewData = data;
        }

        public void SetEvaluator(Func<IViewData, ComponentStatus, ComponentStatus> evaluator)
        {
            Debug.Assert(evaluator != null);
            Evaluator = evaluator;
        }

        public void SetDraw(Func<UIBehaviour, IViewData, ComponentStatus, UIBehaviour> draw)
        {
            Debug.Assert(draw != null);
            Draw = draw;
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"> 해당 액션을 정의한 클래스의 출처 </typeparam>
        /// <param name="action"></param>
        public void SetAction<T>(Action action) where T : class
        {
            Debug.Assert(action != null);
            Action.SetAction<T>(action);
        }
    }
}
