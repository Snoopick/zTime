using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace Pathfinding.Examples {
	/// <summary>Helper script in the example scene 'Turn Based'</summary>
	[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_examples_1_1_turn_based_manager.php")]
	public class TurnBasedManager : MonoBehaviour {
		TurnBasedAI selected;
		
		[SerializeField] private GameObject gameCanvas;
		[SerializeField] private GameObject menuCanvas;
		[SerializeField] private GameObject EndGameCanvas;
		[SerializeField] private GameObject exitZone; 
		[SerializeField] private int needFindItems = 1;
		public int findedItems = 0;
		public float movementSpeed;
		public GameObject nodePrefab;
		public LayerMask layerMask;
		private GameObject selectedNode;
		List<GameObject> possibleMoves = new List<GameObject>();
		EventSystem eventSystem;
		public State state = State.SelectUnit;
		[SerializeField] private GameObject[] players;
		public GameObject[] enemys;
		private GameObject nearestUnit;
		private GameObject nearestHex;
		private TurnBasedAI selectedEnemy;
		
		private Coroutine attackProcessCoroutine = null;
		private Coroutine moveProcessCoroutine = null;
		private Coroutine setEnemyCoroutine = null;

		public GameObject BulletPrefab;
		public GameObject shootEffect;
		public float reloadTimer = 0f;
		public const float reloadCooldown = 0.1f;
		public GameObject shootPoint;
		private bool isUpdateSpawners = false;
		private bool isEnemyMove = false;
		private bool issetPlayerCamers = false;
		private bool hasAlivePlayersUnit = true;
		public bool hardMode = false;
		[SerializeField] private Texture2D cursorTexture;

		public enum State {
			SelectUnit,
			SelectTarget,
			Move,
			EnemyMove,
			EnemyAttack,
			SetEnemy,
			EndEnemyTurn,
			EndGame,
		}

		void Awake () {
			eventSystem = FindObjectOfType<EventSystem>();
		}

		private void Start()
		{
			players = GameObject.FindGameObjectsWithTag("Player");
			GetEnemies();
			SetCursor();
		}

		void Update ()
		{
			if (Input.GetKeyDown(KeyCode.Mouse1))
			{
				DestroyPossibleMoves();
				selected = null;
			}
			
			for (int i = 0; i < players.Length; i++)
			{
				if (players[i].GetComponent<UnitHealth>().health < 1)
				{
					hasAlivePlayersUnit = false;
				}
			}

			if (!hasAlivePlayersUnit)
			{
				state = State.EndGame;
			}

			var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			if (findedItems >= needFindItems)
			{
				exitZone.SetActive(true);
			}
			
			// Ignore any input while the mouse is over a UI element
			if (eventSystem.IsPointerOverGameObject()) {
				return;
			}

			if (state == State.SelectTarget) {
				HandleButtonUnderRay(ray);
			}

			if (state == State.SelectUnit || state == State.SelectTarget) {
				if (isUpdateSpawners)
				{
					EnemySpawner[] objs = GameObject.FindObjectsOfType<EnemySpawner>();
					for (int i = 0; i < objs.Length; i++)
					{
						objs[i].isSettedInTurn = false;
					}

					isUpdateSpawners = false;
				}
				
				GameObject.Find("MainCamera").GetComponent<MainCamera>().target = null;
				
				if (Input.GetKeyDown(KeyCode.Mouse0)) {
					var unitUnderMouse = GetByRay<TurnBasedAI>(ray);

					if (unitUnderMouse != null && unitUnderMouse.movementPoints > 0 && unitUnderMouse.tag != "Enemy") {
						var unitHealth = unitUnderMouse.GetComponent<UnitHealth>();
						
						if (unitHealth.health > 0)
						{
							Select(unitUnderMouse);
							DestroyPossibleMoves();
							GeneratePossibleMoves(selected);
							state = State.SelectTarget;
						}
					}
				}
			}

			if (state == State.EnemyMove)
			{
				DestroyPossibleMoves();
				if (isEnemyMove)
				{
					return;
				}
				
				FindNearestUnit();
				GetEnemies();
				int enemiesWhoCanMove = 0;

				if (enemys != null)
				{
					GameObject[] hexes = null;
					for (int i = 0; i < enemys.Length; i++)
					{
						enemiesWhoCanMove = 0;
						isEnemyMove = true;

						hexes = null;
						nearestHex = null;
						Select(enemys[i].GetComponent<TurnBasedAI>());

						if (selected.canMove)
						{
							enemiesWhoCanMove++;
							break;
						}
					}
					
					if (enemiesWhoCanMove > 0) {
						isEnemyMove = true;
						GameObject.Find("MainCamera").GetComponent<MainCamera>().target = selected.transform;
						
						selected.movementPoints = 8;

						DestroyPossibleMoves();
						GeneratePossibleMoves(selected);
						
						hexes = GameObject.FindGameObjectsWithTag("Hex");
						float distance = Mathf.Infinity;
						
						for (int j = 0; j < hexes.Length; j++)
						{
							if (nearestHex == null)
							{
								nearestHex = hexes[j];
							}
							else
							{
								Vector3 diff = hexes[j].transform.position - nearestUnit.transform.position;
								float curDistance = diff.sqrMagnitude;

								if (curDistance < distance)
								{
									nearestHex = hexes[j];
									distance = curDistance;
								}
							}
						}

						if (nearestHex != null)
						{
							Astar3DButton button = nearestHex.GetComponent<Astar3DButton>();
							selected.canMove = false;
							HandleButton(button);
						}
						else
						{
							Debug.LogError("No Hexes");
						}
					}
					else
					{
						isEnemyMove = false;
						state = State.EnemyAttack;
					}
				}
				else
				{
					state = State.SetEnemy;
				}
			}

			if (state == State.EnemyAttack)
			{
				GetEnemies();
				StartCoroutine(EnemyAttack());
			}

			if (state == State.SetEnemy)
			{
				if (setEnemyCoroutine == null)
				{
					setEnemyCoroutine = StartCoroutine(SetEnemies());					
				}

				
				state = State.EndEnemyTurn;
			}

			if (state == State.EndEnemyTurn)
			{
				GameObject.Find("MainCamera").GetComponent<MainCamera>().target = players[0].transform;
				state = State.SelectUnit;
			}

			if (state == State.EndGame)
			{
				gameCanvas.SetActive(false);
				menuCanvas.SetActive(false);
				EndGameCanvas.SetActive(true);
			}
		}
		
		void HandleButton (Astar3DButton button) {
			if (button.node != null) {
				button.OnClick();
				DestroyPossibleMoves();

				TurnToPoint(button.transform);
				moveProcessCoroutine = StartCoroutine(MoveToNode(selected, button.node, State.EnemyMove));
			}
		}

		// TODO: Move to separate class
		void HandleButtonUnderRay (Ray ray) {
			var button = GetByRay<Astar3DButton>(ray);

			if (button != null && Input.GetKeyDown(KeyCode.Mouse0)) {
				button.OnClick();

				DestroyPossibleMoves();
				state = State.Move;
				
				TurnToPoint(button.transform);
				StartCoroutine(MoveToNode(selected, button.node, State.SelectUnit));
			}
			
			
			var enemy = GetByRay<TurnBasedAI>(ray);
			selectedEnemy = enemy;

			if (enemy != null && enemy.tag == "Enemy" && Input.GetKeyDown(KeyCode.Mouse0))
			{
				RaycastHit hit;

				Vector3 nv = new Vector3(selected.transform.position.x, 1.5f, selected.transform.position.z);

				// подумать на потом
				Ray rRay = new Ray(nv, (selectedEnemy.transform.position - selected.transform.position).normalized);
				Physics.Raycast(rRay, out hit);

				if (hit.collider != null)
				{
					if (Physics.Linecast(shootPoint.transform.position, selectedEnemy.transform.position, out hit))
					{
						if (hit.collider.gameObject != selectedEnemy.gameObject)
						{
							Debug.Log("Путь к врагу преграждает объект: " + hit.collider.name + "["+hit.transform.position+"]");
						}
						else
						{
							Debug.Log("Можно стрелять");
						}

						//просто для наглядности рисуем луч в окне Scene
						Debug.DrawLine(ray.origin, hit.point, Color.red);
					}
				}
				else
				{
					Debug.Log("Нет хита от рейкаста");
				}

				float distance = Mathf.Infinity;
				Vector3 diff = selected.transform.position - enemy.gameObject.transform.position;
				float curDistance = diff.sqrMagnitude;

				if (curDistance < distance)
				{
					distance = curDistance;
				}

				if (distance < 350 && selected.canAttack)
				{
					selected.canAttack = false;
					TurnToPoint(selectedEnemy.transform);
					StartCoroutine(UnitAttack());
				}
			}

			var box = GetByRay<Box>(ray);
			if (box != null && box.tag == "Box" && Input.GetKeyDown(KeyCode.Mouse0))
			{
				var bDistance = Vector3.Distance(selected.transform.position, box.transform.position);
				if (bDistance < 1.5f)
				{
					TurnToPoint(box.transform);
					box.isOpened = true;					
				}
			}
			
			var pickupItem = GetByRay<Loot>(ray);
			if (pickupItem != null && Input.GetKeyDown(KeyCode.Mouse0))
			{
				Debug.Log(pickupItem.name);
				var bDistance = Vector3.Distance(selected.transform.position, box.transform.position);
				if (bDistance < 1.5f)
				{
					TurnToPoint(pickupItem.transform);
					pickupItem.PickUp();
				}
			}
		}

		T GetByRay<T>(Ray ray) where T : class {
			RaycastHit hit;

			if (Physics.Raycast(ray, out hit, float.PositiveInfinity, layerMask)) {
				return hit.transform.GetComponentInParent<T>();
			}
			return null;
		}

		void Select (TurnBasedAI unit) {
			selected = unit;
		}

		IEnumerator MoveToNode (TurnBasedAI unit, GraphNode node, State nextState) {
			var path = ABPath.Construct(unit.transform.position, (Vector3)node.position);

			path.traversalProvider = unit.traversalProvider;

			// Schedule the path for calculation
			AstarPath.StartPath(path);

			// Wait for the path calculation to complete
			yield return StartCoroutine(path.WaitForPath());

			if (path.error) {
				// Not obvious what to do here, but show the possible moves again
				// and let the player choose another target node
				// Likely a node was blocked between the possible moves being
				// generated and the player choosing which node to move to
				Debug.LogError("Path failed:\n" + path.errorLog);
				state = nextState;
				moveProcessCoroutine = null;
				isEnemyMove = false;
				yield break;
			}

			// Set the target node so other scripts know which
			// node is the end point in the path
			unit.targetNode = path.path[path.path.Count - 1];
			selected.GetComponent<UnitSoundManager>().PlayStepSound();
			yield return StartCoroutine(MoveAlongPath(unit, path, movementSpeed));

			selected.GetComponent<UnitSoundManager>().StopPlayStepSound();
			unit.blocker.BlockAtCurrentPosition();

			// Select a new unit to move
			state = nextState;
			moveProcessCoroutine = null;
			isEnemyMove = false;
		}
		
		IEnumerator EnemyAttack () {
			if (enemys != null)
			{
				for (int i = 0; i < enemys.Length; i++)
				{
					Select(enemys[i].GetComponent<TurnBasedAI>());
					selected.canMove = true;

					if (!selected.canAttack)
					{
						continue;
					}
					
					selected.canAttack = false;
					TurnToPoint(nearestUnit.transform);
					var unitHealth = nearestUnit.GetComponent<UnitHealth>();
						
					if (Vector3.Distance(selected.transform.position, nearestUnit.transform.position) < 1.5f && unitHealth.health > 0)
					{
						Animator enemyAnimator = enemys[i].GetComponentInChildren<Animator>();
						GameObject.Find("MainCamera").GetComponent<MainCamera>().target = selected.transform;

						if (enemyAnimator != null)
						{
							selected.GetComponent<UnitSoundManager>().EnemyBite();
							enemyAnimator.SetTrigger("Attack");
							unitHealth.Hit(1);
						}
					}
				}
			}
			
			yield return new WaitForSeconds(0.1f);
			state = State.SetEnemy;
			for (int i = 0; i < enemys.Length; i++)
			{
				Select(enemys[i].GetComponent<TurnBasedAI>());
				selected.canAttack = true;
			}

			GameObject.Find("MainCamera").GetComponent<MainCamera>().target = null;
		}
		
		IEnumerator UnitAttack () {
			shootEffect.SetActive(true);
			if (reloadTimer > 0) reloadTimer -= Time.deltaTime; 
			var unitAnimator = selected.GetComponentInChildren<Animator>();
			
			reloadTimer = reloadCooldown;
			unitAnimator.SetBool("Attack", true);
			selected.GetComponent<UnitSoundManager>().PlayShootSound();

			for (int i = 0; i < 1; i++)
			{
				GameObject instance = Instantiate(BulletPrefab, shootPoint.transform.position, Quaternion.identity);
				instance.GetComponent<Bullet>().Setup(selectedEnemy.transform.position);
				
				if (instance != null)
				{
					Destroy(instance, 4f);
				}
			}

			yield return new WaitForSeconds(.5f);
			
			unitAnimator.SetBool("Attack", false);
			shootEffect.SetActive(false);
		}
		
		IEnumerator SetEnemies () {
			EnemySpawner[] objs = GameObject.FindObjectsOfType<EnemySpawner>();
			for (int i = 0; i < objs.Length; i++)
			{
				objs[i].SetEnemy();
			}					

			yield return new WaitForSeconds(2f);

			setEnemyCoroutine = null;
		}

		/// <summary>Interpolates the unit along the path</summary>
		static IEnumerator MoveAlongPath (TurnBasedAI unit, ABPath path, float speed) {
			if (path.error || path.vectorPath.Count == 0)
				throw new System.ArgumentException("Cannot follow an empty path");

			var unitAnimator = unit.GetComponentInChildren<Animator>();

			// Very simple movement, just interpolate using a catmull rom spline
			float distanceAlongSegment = 0;
			for (int i = 0; i < path.vectorPath.Count - 1; i++) {
				var p0 = path.vectorPath[Mathf.Max(i-1, 0)];
				// Start of current segment
				var p1 = path.vectorPath[i];
				// End of current segment
				var p2 = path.vectorPath[i+1];
				var p3 = path.vectorPath[Mathf.Min(i+2, path.vectorPath.Count-1)];

				var segmentLength = Vector3.Distance(p1, p2);

				while (distanceAlongSegment < segmentLength) {
					var interpolatedPoint = AstarSplines.CatmullRom(p0, p1, p2, p3, distanceAlongSegment / segmentLength);
					unit.transform.position = interpolatedPoint;

					// Locomotion
					/*unitAnimator.SetFloat("MoveForward", 1f);
					unitAnimator.SetFloat("Movement", 1f);*/
					
					// Easy
					unitAnimator.SetInteger("Run", 1);

					yield return null;
					distanceAlongSegment += Time.deltaTime * speed;
				}

				distanceAlongSegment -= segmentLength;
			}

//			TurnToPoint(path.vectorPath[path.vectorPath.Count - 1]);
			unit.transform.position = path.vectorPath[path.vectorPath.Count - 1];
			
			// Locomotion
			/*unitAnimator.SetFloat("MoveForward", 0);
			unitAnimator.SetFloat("Movement", 0f);*/
			
			// Easy
			unitAnimator.SetInteger("Run", 0);

			unit.movementPoints = unit.movementPoints - (path.vectorPath.Count - 1);
		}

		void DestroyPossibleMoves ()
		{
			foreach (var go in possibleMoves) {
				GameObject.Destroy(go);
			}
			possibleMoves.Clear();
		}

		void GeneratePossibleMoves (TurnBasedAI unit) {
			var path = ConstantPath.Construct(unit.transform.position, unit.movementPoints * 1000 + 1);

			path.traversalProvider = unit.traversalProvider;

			// Schedule the path for calculation
			AstarPath.StartPath(path);

			// Force the path request to complete immediately
			// This assumes the graph is small enough that
			// this will not cause any lag
			path.BlockUntilCalculated();

			foreach (var node in path.allNodes) {
				if (node != path.startNode) {
					// Create a new node prefab to indicate a node that can be reached
					// NOTE: If you are going to use this in a real game, you might want to
					// use an object pool to avoid instantiating new GameObjects all the time
					var go = Instantiate(nodePrefab, (Vector3)node.position, Quaternion.identity);
					possibleMoves.Add(go);

					go.GetComponent<Astar3DButton>().node = node;
				}
			}
		}
		
		void FindNearestUnit()
		{
			if (players != null)
			{
				for (int i = 0; i < players.Length; i++)
				{
					if (nearestUnit == null)
					{
						nearestUnit = players[i];
					}
				}
			}
		}

		public void GetEnemies()
		{
			enemys = null;
			enemys = GameObject.FindGameObjectsWithTag("Enemy");
		}

		public void UpdateTurn()
		{
			if (players != null)
			{
				for (int i = 0; i < players.Length; i++)
				{
					players[i].GetComponent<TurnBasedAI>().movementPoints = 10;
					players[i].GetComponent<TurnBasedAI>().canAttack = true;
				}
			}
		}

		private void TurnToPoint(Transform point)
		{
			Vector3 ea = transform.eulerAngles;
			selected.transform.LookAt(point);
			selected.transform.eulerAngles = new Vector3(ea.x, selected.transform.eulerAngles.y, ea.z);
		}

		private void SetCursor()
		{
			CursorMode mode = CursorMode.ForceSoftware;
			Vector2 hotSpot = new Vector2(0,0);
			Cursor.SetCursor(cursorTexture, hotSpot, mode);
		}
	}
}
