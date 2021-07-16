// Author: Zhujiamin
// Email: ilclpj@163.com
// QQ: 233423144
// Time: 2021-07-15 14:10
// Description:

using UnityEngine;
using UnityEngine.UI;

namespace ilclpj.Components
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
        [Header("覆盖父节点指定的baseOrder")]
        [Tooltip("设置后的baseOrder由以下决定:\n1.添加组件时的sorting\n2.代码设置")]
        public bool overrideSorting;

        private void Awake()
        {
            var render = GetComponent<Renderer>();
            if (null != render)
            {
                curOrder = render.sortingOrder;
            }
            else
            {
                var canvas = gameObject.GetComponent<Canvas>();
                if (null == canvas)
                {
                    canvas = gameObject.AddComponent<Canvas>();
                    gameObject.AddComponent<GraphicRaycaster>();
                }

                canvas.overrideSorting = true;

                curOrder = canvas.sortingOrder;
            }

            baseOrder = curOrder;
        }

        private void _SetOrder(Component trans, int order)
        {
            var render = trans.GetComponent<Renderer>();
            if (null != render)
            {
                render.sortingOrder = order;
                return;
            }

            var canvas = trans.GetComponent<Canvas>();
            if (null == canvas) return;

            canvas.overrideSorting = true;
            canvas.sortingOrder = order;
        }

        private void _SetAllChildrenOrder(Transform trans, int order)
        {
            for (var i = 0; i < trans.childCount; i++)
            {
                var child = trans.GetChild(i);
                var renderSortingOrder = child.GetComponent<RenderSortingOrder>();
                if (null != renderSortingOrder)
                {
                    if (!renderSortingOrder.overrideSorting)
                    {
                        renderSortingOrder.SetBaseOrder(order);
                    }

                    renderSortingOrder.RefreshAllOrder();

                    continue;
                }

                _SetOrder(child, order);
                _SetAllChildrenOrder(child, order);
            }
        }

        public void SetBaseOrder(int order, bool autoRefresh = true)
        {
            baseOrder = order;

            if (autoRefresh)
                RefreshOrder();
        }

        public void RefreshOrder()
        {
            curOrder = baseOrder + orderDelta;
            _SetOrder(transform, curOrder);
        }

        [ContextMenu("设置所有子节点的SoringOrder(与自己保持一致)")]
        public void RefreshAllOrder()
        {
            RefreshOrder();
            _SetAllChildrenOrder(transform, curOrder);
        }
    }
}