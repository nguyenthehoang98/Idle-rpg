from bridge import ALLOWED_OPERATIONS, BridgeState


def main() -> None:
    state = BridgeState("test-token")
    state.enqueue({"id": "1", "op": "create_frame", "args": {"name": "Draft"}})
    command = state.next_command()
    assert command == {"id": "1", "op": "create_frame", "args": {"name": "Draft"}}
    assert state.next_command() is None
    assert "create_frame" in ALLOWED_OPERATIONS
    assert "set_reactions" in ALLOWED_OPERATIONS
    assert "delete_all" not in ALLOWED_OPERATIONS
    print("bridge self-check passed")


if __name__ == "__main__":
    main()
