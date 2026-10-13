extends CanvasLayer
## Controles táctiles: joystick a la izquierda, botones de acción a la derecha y pausa arriba.
## Solo se muestran en pantallas táctiles. Para probarlos en el PC, activa `always_visible`.

@export var always_visible := false


func _ready() -> void:
	visible = always_visible or Settings.uses_touch()
	if not visible:
		# Sin pantalla táctil sobran: así no capturan clics del ratón.
		for child in $Root.get_children():
			child.process_mode = Node.PROCESS_MODE_DISABLED
