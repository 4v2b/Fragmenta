export function Task({ item, columnId }) {
    return (
      <div
        ref={setRef}
        className="border p-2 mb-2 bg-white rounded cursor-move"
      >
        {item.content}
      </div>
    );
  };