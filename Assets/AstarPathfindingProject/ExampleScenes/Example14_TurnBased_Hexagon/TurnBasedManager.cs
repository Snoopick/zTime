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
		[SerializeField] private GameObject exitZone; 
		[SerializeField] private int needFindItems = 1;
		[SerializeField] private int findedItems = 0;
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

		public enum State {
			SelectUnit,
			SelectTarget,
			Move,
			EnemyMove,
			EnemyAttack,
			SetEnemy,
		}

		void Awake () {
			eventSystem = FindObjectOfType<EventSystem>();
		}

		private void Start()
		{
			players = GameObject.FindGameObjectsWithTag("Player");
			GetEnemies();
		}

		void Update () {
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
				bool isEnemyMove = true;
				
				FindNearestUnit();
				GetEnemies();

				if (enemys != null)
				{
					GameObject[] hexes = null;

					while (isEnemyMove)
					{
						int enemiesWhoCanMove = 0;
						for (int i = 0; i < enemys.Length; i++)
						{
							hexes = null;
							nearestHex = null;
							Select(enemys[i].GetComponent<TurnBasedAI>());

							if (!selected.canMove)
							{
								continue;
							}

							enemiesWhoCanMove++;
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

						if (enemiesWhoCanMove == 0)
						{
							isEnemyMove = false;
							state = State.EnemyAttack;
						}
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

				state = State.SelectUnit;
			}
		}
		
		void HandleButton (Astar3DButton button) {
			if (button.node != null) {
				button.OnClick();
				DestroyPossibleMoves();

				TurnToPoint(button.transform);

//				if (moveProcessCoroutine != null)
//				{
					moveProcessCoroutine = StartCoroutine(MoveToNode(selected, button.node, State.EnemyAttack));
//				}
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
				Ray rRay = new Ray(selected.transform.position, selectedEnemy.transform.position - selected.transform.position);
				Physics.Raycast(rRay, out hit);
				Debug.DrawLine(ray.origin, hit.point, Color.red);
				
				if (hit.collider != null){
					//если луч не попал в цель
					if (hit.collider.gameObject != selectedEnemy.gameObject){
						Debug.Log("Путь к врагу преграждает объект: "+hit.collider.name);
					}   
					//если луч попал в цель
					else
					{
						Debug.Log("Попадаю во врага!!!");
					}
					//просто для наглядности рисуем луч в окне Scene
					Debug.DrawLine(ray.origin, hit.point,Color.red);
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
//				GeneratePossibleMoves(selected);
				moveProcessCoroutine = null;
				yield break;
			}

			// Set the target node so other scripts know which
			// node is the end point in the path
			unit.targetNode = path.path[path.path.Count - 1];

			yield return StartCoroutine(MoveAlongPath(unit, path, movementSpeed));

			unit.blocker.BlockAtCurrentPosition();

			// Select a new unit to move
			state = nextState;
			moveProcessCoroutine = null;
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

						if (enemyAnimator != null)
						{
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
		}
		
		IEnumerator UnitAttack () {
			shootEffect.SetActive(true);
			if (reloadTimer > 0) reloadTimer -= Time.deltaTime; 
			var unitAnimator = selected.GetComponentInChildren<Animator>();
			
			reloadTimer = reloadCooldown;
			unitAnimator.SetBool("Attack", true);

			for (int i = 0; i < 5; i++)
			{
				var instance = Instantiate(BulletPrefab, shootPoint.transform.position, Quaternion.identity);

				Vector3 ea = transform.eulerAngles;
				instance.transform.LookAt(selectedEnemy.transform);
				instance.transform.eulerAngles = new Vector3(ea.x, instance.transform.eulerAngles.y, ea.z);

				instance.transform.Translate(instance.transform.forward * 0.5f);
				var rig = instance.GetComponent<Rigidbody>();

				if (rig)
				{
					rig.velocity = Vector3.zero;
					rig.AddForce(instance.transform.forward * 50f, ForceMode.Impulse);
					
				}

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
	}
}
