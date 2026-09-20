using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OrbitalAuthority.Rendering.UI.HUD;

namespace OrbitalAuthority.Rendering.Renderers;

public sealed class ObjectListRenderer
{
    private const int PanelWidth = 220;
    private const int PanelX = 10;
    private const int PanelY = 200;
    private const int HeaderHeight = 26;
    private const int RowHeight = 20;

    private readonly SpriteFont _font;
    private Texture2D _pixel;
    private List<ObjectListItem> _items = new();
    private int _selectedIndex = -1;
    private int _hoveredIndex = -1;
    private bool _expanded;

    public int SelectedEntityId =>
        _selectedIndex >= 0 && _selectedIndex < _items.Count
            ? _items[_selectedIndex].EntityId
            : -1;

    public ObjectListRenderer(GraphicsDevice device, SpriteFont font)
    {
        _font = font;
        _pixel = new Texture2D(device, 1, 1);
        _pixel.SetData(new[] { Color.White });
    }

    /// <summary>Вызывать один раз при создании мира.</summary>
    public void SetItems(List<ObjectListItem> items)
    {
        _items = items;
        _selectedIndex = _items.Count > 0 ? 0 : -1;
    }

    public void SelectNext()
    {
        if (_items.Count == 0) return;
        _selectedIndex = (_selectedIndex + 1) % _items.Count;
    }

    public void SelectPrev()
    {
        if (_items.Count == 0) return;
        _selectedIndex = (_selectedIndex - 1 + _items.Count) % _items.Count;
    }

    public void ClearSelection()
    {
        _selectedIndex = -1;
        _expanded = false;
    }

    /// <summary>
    /// Обрабатывает мышь: наведение, разворачивание/сворачивание списка и клик по объекту.
    /// Возвращает true, если выбранный объект изменился кликом.
    /// </summary>
    public bool HandleInput(MouseState mouse, MouseState prevMouse, int screenWidth, int screenHeight)
    {
        var layout = ComputeLayout(screenHeight);
        var mousePos = new Point(mouse.X, mouse.Y);

        _hoveredIndex = -1;
        if (_expanded)
        {
            for (int i = 0; i < layout.Items.Count; i++)
            {
                if (layout.Items[i].Contains(mousePos))
                {
                    _hoveredIndex = i;
                    break;
                }
            }
        }

        bool leftClicked = mouse.LeftButton == ButtonState.Pressed &&
                            prevMouse.LeftButton == ButtonState.Released;
        if (!leftClicked)
            return false;

        if (layout.Header.Contains(mousePos))
        {
            _expanded = !_expanded;
            return false;
        }

        if (_expanded)
        {
            for (int i = 0; i < layout.Items.Count; i++)
            {
                if (!layout.Items[i].Contains(mousePos)) continue;

                _selectedIndex = i;
                _expanded = false;
                return true;
            }

            // Клик вне панели — закрыть список без изменения выбора.
            if (!layout.Panel.Contains(mousePos))
            {
                _expanded = false;
            }
        }

        return false;
    }

    public void Draw(SpriteBatch spriteBatch, int screenWidth, int screenHeight)
    {
        var layout = ComputeLayout(screenHeight);

        spriteBatch.Draw(_pixel, layout.Panel, new Color(0, 0, 0, 180));
        spriteBatch.Draw(_pixel, layout.Header, new Color(30, 30, 45, 210));

        string arrow = _expanded ? "[-]" : "[+]";
        string selectedName = _selectedIndex >= 0 && _selectedIndex < _items.Count
            ? _items[_selectedIndex].Name
            : "none";
        spriteBatch.DrawString(_font, $"{arrow} Object: {selectedName}",
            new Vector2(layout.Header.X + 8, layout.Header.Y + 5), Color.Yellow);

        if (!_expanded) return;

        for (int i = 0; i < layout.Items.Count; i++)
        {
            var rect = layout.Items[i];

            if (i == _selectedIndex)
                spriteBatch.Draw(_pixel, rect, new Color(90, 90, 20, 200));
            else if (i == _hoveredIndex)
                spriteBatch.Draw(_pixel, rect, new Color(55, 55, 70, 200));

            var textColor = i == _selectedIndex ? Color.Yellow
                : i == _hoveredIndex ? Color.Orange
                : Color.White;

            spriteBatch.DrawString(_font, _items[i].Name,
                new Vector2(rect.X + 10, rect.Y + 2), textColor);
        }
    }

    private Layout ComputeLayout(int screenHeight)
    {
        var header = new Rectangle(PanelX, PanelY, PanelWidth, HeaderHeight);
        var items = new List<Rectangle>();
        int panelHeight = HeaderHeight;

        if (_expanded && _items.Count > 0)
        {
            int maxListHeight = Math.Max(0, screenHeight - PanelY - HeaderHeight - 10);
            int visibleCount = Math.Min(_items.Count, maxListHeight / RowHeight);

            for (int i = 0; i < visibleCount; i++)
            {
                items.Add(new Rectangle(PanelX, PanelY + HeaderHeight + i * RowHeight, PanelWidth, RowHeight));
            }

            panelHeight = HeaderHeight + visibleCount * RowHeight;
        }

        var panel = new Rectangle(PanelX, PanelY, PanelWidth, panelHeight);
        return new Layout(header, panel, items);
    }

    public void Dispose() => _pixel?.Dispose();

    private readonly struct Layout
    {
        public readonly Rectangle Header;
        public readonly Rectangle Panel;
        public readonly List<Rectangle> Items;

        public Layout(Rectangle header, Rectangle panel, List<Rectangle> items)
        {
            Header = header;
            Panel = panel;
            Items = items;
        }
    }
}
