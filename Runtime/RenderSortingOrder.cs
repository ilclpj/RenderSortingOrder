using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace LF.Components
{
    public class RenderSortingOrder : MonoBehaviour
    {
        [Header("基准Order")]
        public int baseOrder;

        [Header("当前的Order")]
        public int curOrder;

        [Header("偏移")]
        public int orderDelta;

        /// <summary>
        /// 覆盖父节点指定的baseOrder
        /// </summary>
        [Header("父节刷新order时忽略本节点和所有子节点")]
        public bool overrideSorting = true;

        private bool m_HasInit;
        private Canvas m_Canvas;
        private Renderer m_Renderer;

        public void AddInitComponents()
        {
            if (m_HasInit) return;
            m_HasInit = true;

            m_Renderer = GetComponent<Renderer>();
            if (null == m_Renderer)
            {
                m_Canvas = gameObject.GetComponent<Canvas>();
                if (null == m_Canvas)
                {
                    m_Canvas = gameObject.AddComponent<Canvas>();
                    gameObject.AddComponent<GraphicRaycaster>();

                    m_Canvas.overrideSorting = true;
                }
            }
        }

        public void SetSelfOrder()
        {
            if (m_Renderer == null)
                m_Renderer = GetComponent<Renderer>();

            if (m_Renderer != null)
            {
                m_Renderer.sortingOrder = curOrder;
                return;
            }

            if (m_Canvas == null)
                m_Canvas = GetComponent<Canvas>();

            if (m_Canvas != null)
            {
                m_Canvas.overrideSorting = true;
                m_Canvas.sortingOrder = curOrder;
            }
        }

        public void SetBaseOrder(int order, bool refresh = false)
        {
            baseOrder = order;
            curOrder = baseOrder + orderDelta;

            if (refresh)
            {
                if (!m_HasInit)
                    AddInitComponents();

                SetSelfOrder();
            }
        }

        // 查找和缓存自身所有的canvas和renderer和renderSortingOrder
        // 缓存后加速索引
        private void _FindAllComponents(out Dictionary<Transform, Canvas> allCanvas, out Dictionary<Transform, Renderer> allRenderer, out Dictionary<Transform, RenderSortingOrder> allRendererSortingOrder)
        {
            allCanvas = transform.GetComponentsInChildren<Canvas>(true).ToDictionary(component => component.transform);
            allRenderer = transform.GetComponentsInChildren<Renderer>(true).ToDictionary(component => component.transform);
            allRendererSortingOrder = transform.GetComponentsInChildren<RenderSortingOrder>(true).ToDictionary(component => component.transform);
        }

        private int _SetNodeOrder(int order, Transform trans, IDictionary<Transform, Canvas> allCanvas,
            IDictionary<Transform, Renderer> allRenderer, IDictionary<Transform, RenderSortingOrder> allRenderSortingOrder)
        {
            RenderSortingOrder renderSortingOrder;
            var exist = allRenderSortingOrder.TryGetValue(trans, out renderSortingOrder);
            if (exist)
            {
                // 如果overrideSorting为false才使用新的order来更新, 否则使用自身已存在的order
                renderSortingOrder.SetBaseOrder(renderSortingOrder.overrideSorting ? renderSortingOrder.baseOrder : order);
                order = renderSortingOrder.curOrder;
            }

            Renderer render;
            exist = allRenderer.TryGetValue(trans, out render);
            if (exist)
            {
                render.sortingOrder = order;
                return order;
            }

            Canvas canvas;
            exist = allCanvas.TryGetValue(trans, out canvas);
            if (!exist) return order;

            canvas.overrideSorting = true;
            canvas.sortingOrder = order;

            return order;
        }


        [ContextMenu("设置所有子节点的SoringOrder(与自己保持一致)")]
        public void RefreshAllOrder()
        {
            //--------------------------------------------------------------------
            Dictionary<Transform, Canvas> allCanvas;
            Dictionary<Transform, Renderer> allRenderer;
            Dictionary<Transform, RenderSortingOrder> allRendererSortingOrder;
            _FindAllComponents(out allCanvas, out allRenderer, out allRendererSortingOrder);
            //--------------------------------------------------------------------

            var queue = new Queue<KeyValuePair<int, Transform>>();
            // var node = new Node(curOrder, transform);
            var node = new KeyValuePair<int, Transform>(curOrder, transform);
            queue.Enqueue(node);

            while (queue.Count > 0)
            {
                var cur = queue.Dequeue();
                var order = cur.Key;

                for (var i = cur.Value.childCount - 1; i >= 0; i--)
                {
                    var child = cur.Value.GetChild(i);
                    var newOrder = _SetNodeOrder(order, child, allCanvas, allRenderer, allRendererSortingOrder);
                    if (child.childCount <= 0) continue;

                    var childNode = new KeyValuePair<int, Transform>(newOrder, child);
                    queue.Enqueue(childNode);
                }
            }
        }
    }
}