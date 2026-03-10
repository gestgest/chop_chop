using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;


/// <summary>
/// 마우스 focus와 키보드입력을 동시에 해서 입력이 무시되는 현상을 해결하고자 만든 핸들러
/// </summary>
public class MenuSelectionHandler : MonoBehaviour
{
	[SerializeField] private InputReader _inputReader;
	[SerializeField][ReadOnly] private GameObject _defaultSelection;
	[SerializeField][ReadOnly] private GameObject _currentSelection;
	[SerializeField][ReadOnly] private GameObject _mouseSelection;

	private void OnEnable()
	{
		_inputReader.MoveSelectionEvent += HandleMoveSelection;
		_inputReader.MenuMouseMoveEvent += HandleMoveCursor;

		StartCoroutine(SelectDefault());
	}

	private void OnDisable()
	{
		_inputReader.MoveSelectionEvent -= HandleMoveSelection;
		_inputReader.MenuMouseMoveEvent -= HandleMoveCursor;
	}

	public void UpdateDefault(GameObject newDefault)
	{
		_defaultSelection = newDefault;
	}

	/// <summary>
	/// Highlights the default element. [focus ui]
	/// </summary>
	private IEnumerator SelectDefault()
	{
		// null로 쓰면 안되냐 : 만약 vertical layout인 경우 1프레임보다는 오래 걸린다.
		// Necessary wait otherwise the highlight won't show up
		yield return new WaitForSeconds(.03f);// =>2~3프레임

		//focus
		if (_defaultSelection != null)
			UpdateSelection(_defaultSelection);
	}

	public void Unselect()
	{
		_currentSelection = null;
		if (EventSystem.current != null)
			EventSystem.current.SetSelectedGameObject(null);
	}

	/// <summary>
	/// **Fired by keyboard and gamepad inputs.**
	/// Current selected UI element will be the ui Element that was selected
	/// when the event was fired. The _currentSelection is updated later on, after the EventSystem moves to the
	/// desired UI element, the UI element will call into UpdateSelection()
	/// </summary>
	private void HandleMoveSelection()
	{
		//만약 키보드를 누르면 자동으로 마우스는 안 보이게 함
		Cursor.visible = false;

		// 커서가 UI 바깥에 있는 경우 => 당연히 커서가 이미 물체에 있고 키보드를 아래로 누른다면
		// 대체로 발동은 안하는 듯
		// Handle case where no UI element is selected because mouse left selectable bounds
		if (EventSystem.current.currentSelectedGameObject == null)
		{
			EventSystem.current.SetSelectedGameObject(_currentSelection);
		}
	}

	private void HandleMoveCursor()
	{
		//마우스 선택한 게 잇다면
		if (_mouseSelection != null)
		{
			EventSystem.current.SetSelectedGameObject(_mouseSelection);
		}

		Cursor.visible = true;
	}

	public void HandleMouseEnter(GameObject UIElement)
	{
		_mouseSelection = UIElement;
		EventSystem.current.SetSelectedGameObject(UIElement);
	}

	public void HandleMouseExit(GameObject UIElement)
	{
		if (EventSystem.current.currentSelectedGameObject != UIElement)
		{
			return;
		}

		// keep selecting the last thing the mouse has selected
		_mouseSelection = null;
		EventSystem.current.SetSelectedGameObject(_currentSelection);
	}

	/// <summary>
	/// Method interactable UI elements should call on Submit interaction to determine whether to continue or not.
	/// </summary>
	/// <returns></returns>
	public bool AllowsSubmit()
	{
		// if LMB is not down, there is no edge case to handle, allow the event to continue
		return !_inputReader.LeftMouseDown()
			   // if we know mouse & keyboard are on different elements, do not allow interaction to continue
			   || _mouseSelection != null && _mouseSelection == _currentSelection;
	}

	/// <summary>
	/// Fired by gamepad or keyboard navigation inputs
	/// </summary>
	/// <param name="UIElement"></param>
	public void UpdateSelection(GameObject UIElement)
	{
		//ui element가 chopchop에서 만든 선택 가능 멀티UI가 있어야지 선택 변수에 넣을 수 있음
		if ((UIElement.GetComponent<MultiInputSelectableElement>() != null)
		    || (UIElement.GetComponent<MultiInputButton>() != null))
		{
			_mouseSelection = UIElement;
			_currentSelection = UIElement;
		}
	}

	// Debug
	// private void OnGUI()
	// {
	//	 	GUILayout.Box($"_currentSelection: {(_currentSelection != null ? _currentSelection.name : "null")}");
	//	 	GUILayout.Box($"_mouseSelection: {(_mouseSelection != null ? _mouseSelection.name : "null")}");
	// }
	private void Update()
	{
		if ((EventSystem.current != null) && (EventSystem.current.currentSelectedGameObject == null) && (_currentSelection != null))
		{

			EventSystem.current.SetSelectedGameObject(_currentSelection);
		}
	}
}
