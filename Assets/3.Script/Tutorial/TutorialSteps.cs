using System;
using UnityEngine;

/// <summary>Step 1. 광산으로 이동 — 광석을 하나라도 캐면 완료</summary>
public class Step_GoMine : ITutorialStep
{
    private readonly StackSystem stackSystem;
    private readonly TutorialArrowUI arrow;
    private readonly Vector3 targetPos;

    public bool IsComplete => stackSystem.HasItem(StackItemType.Ore);

    public Step_GoMine(StackSystem stackSystem, TutorialArrowUI arrow, Vector3 targetPos)
    {
        this.stackSystem = stackSystem;
        this.arrow = arrow;
        this.targetPos = targetPos;
    }

    public void OnEnter() => arrow.SetTarget(targetPos);
    public void OnExit()  => arrow.ClearTarget();
}

/// <summary>Step 2. 광석 투입구로 이동 — 투입구에 광석이 하나라도 들어가면 완료</summary>
public class Step_DepositOre : ITutorialStep
{
    private readonly DepositZone oreDepositZone;
    private readonly TutorialArrowUI arrow;
    private readonly Vector3 targetPos;

    public bool IsComplete => oreDepositZone.Count > 0;

    public Step_DepositOre(DepositZone oreDepositZone, TutorialArrowUI arrow, Vector3 targetPos)
    {
        this.oreDepositZone = oreDepositZone;
        this.arrow = arrow;
        this.targetPos = targetPos;
    }

    public void OnEnter() => arrow.SetTarget(targetPos);
    public void OnExit()  => arrow.ClearTarget();
}

/// <summary>Step 3. 제품 배출구로 이동 — 플레이어가 제품을 하나라도 먹으면 완료</summary>
public class Step_PickupProduct : ITutorialStep
{
    private readonly StackSystem stackSystem;
    private readonly TutorialArrowUI arrow;
    private readonly Vector3 targetPos;

    public bool IsComplete => stackSystem.HasItem(StackItemType.Product);

    public Step_PickupProduct(StackSystem stackSystem, TutorialArrowUI arrow, Vector3 targetPos)
    {
        this.stackSystem = stackSystem;
        this.arrow = arrow;
        this.targetPos = targetPos;
    }

    public void OnEnter() => arrow.SetTarget(targetPos);
    public void OnExit()  => arrow.ClearTarget();
}

/// <summary>Step 4. 계산대 투입구로 이동 — 투입구에 제품이 하나라도 들어가면 완료</summary>
public class Step_CashDeposit : ITutorialStep
{
    private readonly DepositZone cashDepositZone;
    private readonly TutorialArrowUI arrow;
    private readonly Vector3 targetPos;

    public bool IsComplete => cashDepositZone.Count > 0;

    public Step_CashDeposit(DepositZone cashDepositZone, TutorialArrowUI arrow, Vector3 targetPos)
    {
        this.cashDepositZone = cashDepositZone;
        this.arrow = arrow;
        this.targetPos = targetPos;
    }

    public void OnEnter() => arrow.SetTarget(targetPos);
    public void OnExit()  => arrow.ClearTarget();
}

/// <summary>Step 5. 코인 배출구로 이동 — 플레이어가 코인을 하나라도 먹으면 완료</summary>
public class Step_PickupCoin : ITutorialStep
{
    private readonly StackSystem stackSystem;
    private readonly TutorialArrowUI arrow;
    private readonly Vector3 targetPos;

    public bool IsComplete => stackSystem.GetCount(StackItemType.Coin) > 0;

    public Step_PickupCoin(StackSystem stackSystem, TutorialArrowUI arrow, Vector3 targetPos)
    {
        this.stackSystem = stackSystem;
        this.arrow = arrow;
        this.targetPos = targetPos;
    }

    public void OnEnter() => arrow.SetTarget(targetPos);
    public void OnExit()  => arrow.ClearTarget();
}

/// <summary>Step 6. 카메라가 업그레이드 존으로 이동 — 복귀 완료 시 튜토리얼 종료</summary>
public class Step_CameraMove : ITutorialStep
{
    private readonly TutorialCameraController cameraController;
    private readonly Vector3 cameraTargetPos;
    private bool done;

    public bool IsComplete => done;

    public Step_CameraMove(TutorialCameraController cameraController, Vector3 cameraTargetPos)
    {
        this.cameraController = cameraController;
        this.cameraTargetPos = cameraTargetPos;
    }

    public void OnEnter() => cameraController.TriggerMove(cameraTargetPos, () => done = true);
    public void OnExit()  { }
}
