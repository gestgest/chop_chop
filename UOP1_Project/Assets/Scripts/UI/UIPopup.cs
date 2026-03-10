using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;
using UnityEngine.Events;

public enum PopupButtonType
{
    Confirm,
    Cancel,
    Close,
    DoNothing,
}

public enum PopupType
{
    Quit,
    NewGame,
    BackToMenu,
}

public class UIPopup : MonoBehaviour
{
    [SerializeField] private LocalizeStringEvent _titleText = default;
    [SerializeField] private LocalizeStringEvent _descriptionText = default;
    [SerializeField] private Button _buttonClose = default;
    [SerializeField] private UIGenericButton _popupButton1 = default;
    [SerializeField] private UIGenericButton _popupButton2 = default;
    [SerializeField] private InputReader _inputReader = default;

    private PopupType _actualType;

    public event UnityAction<bool> ConfirmationResponseAction;
    public event UnityAction ClosePopupAction;

    private void OnDisable()
    {
        _popupButton2.Clicked -= CancelButtonClicked;
        _popupButton1.Clicked -= ConfirmButtonClicked;
        _inputReader.MenuCloseEvent -= ClosePopupButtonClicked;
    }

    public void SetPopup(PopupType popupType)
    {
	    //ex) Quit
        _actualType = popupType;
        bool isConfirmation = false; //버튼 두 개인 경우
        bool hasExitButton = false;

        //텍스트 가져오기
        _titleText.StringReference.TableEntryReference = _actualType.ToString() + "_Popup_Title";
        _descriptionText.StringReference.TableEntryReference = _actualType.ToString() + "_Popup_Description";
        string tableEntryReferenceConfirm = PopupButtonType.Confirm + "_" + _actualType;
        string tableEntryReferenceCancel = PopupButtonType.Cancel + "_" + _actualType;

        //중첩되는 문장이 많지만 만약 버튼이 하나인 팝업이 생길 수 있으니 유지
        //isSelected : 버튼이 focus 상태인지 아닌지.
        switch (_actualType)
        {
            case PopupType.NewGame:
            case PopupType.BackToMenu:
                isConfirmation = true;
                _popupButton1.SetButton(tableEntryReferenceConfirm, true);
                _popupButton2.SetButton(tableEntryReferenceCancel, false);
                hasExitButton = true;
                break;
            case PopupType.Quit:
                isConfirmation = true;
                _popupButton1.SetButton(tableEntryReferenceConfirm, true);
                _popupButton2.SetButton(tableEntryReferenceCancel, false);
                hasExitButton = false;
                break;
            default:
                isConfirmation = false;
                hasExitButton = false;
                break;
        }

        if (isConfirmation) // needs two button : Is a decision
        {
            _popupButton1.gameObject.SetActive(true);
            _popupButton2.gameObject.SetActive(true);

            _popupButton2.Clicked += CancelButtonClicked;
            _popupButton1.Clicked += ConfirmButtonClicked;
        }
        else // needs only one button : Is an information
        {
            _popupButton1.gameObject.SetActive(true);
            _popupButton2.gameObject.SetActive(false);

            _popupButton1.Clicked += ConfirmButtonClicked;
        }

        //닫는 버튼 : quit는 확인이 나가는 거라 필요없다.
        _buttonClose.gameObject.SetActive(hasExitButton);

        if (hasExitButton) // can exit : Has to take the decision or acknowledge the information
        {
            _inputReader.MenuCloseEvent += ClosePopupButtonClicked;
        }
    }

    public void ClosePopupButtonClicked()
    {
        ClosePopupAction.Invoke();
    }

    void ConfirmButtonClicked()
    {
        ConfirmationResponseAction.Invoke(true);
    }

    void CancelButtonClicked()
    {
        ConfirmationResponseAction.Invoke(false);
    }
}
