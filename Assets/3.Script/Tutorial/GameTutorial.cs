using UnityEngine;

/// <summary>
/// 튜토리얼 스텝을 구성하고 시작하는 진입점.
/// 씬의 레퍼런스를 인스펙터에서 연결 후 Start()에서 자동 실행.
/// </summary>
public class GameTutorial : MonoBehaviour
{
    [Header("Core")]
    [SerializeField] private TutorialManager tutorialManager;
    [SerializeField] private TutorialArrowUI destinationArrow;
    [SerializeField] private StackSystem stackSystem;
    [SerializeField] private TutorialCameraController cameraController;

    [Header("Step 1 - 광산")]
    [SerializeField] private Transform miningZoneTarget;

    [Header("Step 2 - 광석 투입구")]
    [SerializeField] private DepositZone oreDepositZone;
    [SerializeField] private Transform oreDepositTarget;

    [Header("Step 3 - 제품 배출구")]
    [SerializeField] private Transform productPickupTarget;

    [Header("Step 4 - 계산대 투입구")]
    [SerializeField] private DepositZone cashDepositZone;
    [SerializeField] private Transform cashDepositTarget;

    [Header("Step 5 - 코인 배출구")]
    [SerializeField] private Transform coinPickupTarget;

    [Header("Step 6 - 카메라 이동 목표")]
    [SerializeField] private Transform cameraLookTarget;

    private void Start()
    {
        tutorialManager.RegisterStep(new Step_GoMine(stackSystem, destinationArrow, miningZoneTarget.position));
        tutorialManager.RegisterStep(new Step_DepositOre(oreDepositZone, destinationArrow, oreDepositTarget.position));
        tutorialManager.RegisterStep(new Step_PickupProduct(stackSystem, destinationArrow, productPickupTarget.position));
        tutorialManager.RegisterStep(new Step_CashDeposit(cashDepositZone, destinationArrow, cashDepositTarget.position));
        tutorialManager.RegisterStep(new Step_PickupCoin(stackSystem, destinationArrow, coinPickupTarget.position));
        tutorialManager.RegisterStep(new Step_CameraMove(cameraController, cameraLookTarget.position));

        tutorialManager.StartTutorial();
    }
}
