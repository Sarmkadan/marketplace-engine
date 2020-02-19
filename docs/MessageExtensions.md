# MessageExtensions

Provides static utility methods for inspecting and interacting with `Message` objects, specifically around conversation threading, depth calculation, and reply creation. These extensions centralize thread-related logic that would otherwise be scattered across the codebase.

## API

### `IsInThread`

```csharp
public static bool IsInThread(this Message message)
```

Determines whether the given message participates in a conversation thread (i.e., it has a parent message or is itself a parent to replies).

**Parameters:**
- `message` — The `Message` instance to inspect. Must not be `null`.

**Returns:**
- `true` if the message has a non-null `ParentMessageId` or has at least one child reply; `false` otherwise.

**Throws:**
- `ArgumentNullException` when `message` is `null`.

---

### `GetConversationDepth`

```csharp
public static int GetConversationDepth(this Message message)
```

Calculates how many levels deep the message sits within its conversation thread, counting from the root ancestor.

**Parameters:**
- `message` — The `Message` instance to evaluate. Must not be `null`.

**Returns:**
- A zero-based depth value where `0` indicates the message is the root of its thread (no parent). Each level of ancestry increments the depth by one.

**Throws:**
- `ArgumentNullException` when `message` is `null`.
- `InvalidOperationException` if a circular parent reference is detected during traversal.

---

### `GetConversationCount`

```csharp
public static int GetConversationCount(this Message message)
```

Counts the total number of messages within the same conversation thread, including the message itself, all ancestors reachable by walking parent references, and all descendants reachable by walking child references.

**Parameters:**
- `message` — The `Message` instance whose thread to count. Must not be `null`.

**Returns:**
- The total message count in the thread. Returns `1` if the message has no parent and no replies.

**Throws:**
- `ArgumentNullException` when `message` is `null`.
- `InvalidOperationException` if a circular reference is detected in either the ancestor or descendant chain.

---

### `CreateReply`

```csharp
public static Message CreateReply(this Message message, string content, string authorId)
```

Creates a new `Message` instance configured as a direct reply to the given message, with parent-child relationships properly established on both sides.

**Parameters:**
- `message` — The parent `Message` to reply to. Must not be `null`.
- `content` — The body text of the reply. Must not be `null` or empty.
- `authorId` — The identifier of the author creating the reply. Must not be `null` or empty.

**Returns:**
- A new `Message` object with `ParentMessageId` set to the parent's identifier, the parent's reply collection updated to include the new child, and `CreatedAt` set to the current UTC time.

**Throws:**
- `ArgumentNullException` when `message`, `content`, or `authorId` is `null`.
- `ArgumentException` when `content` or `authorId` is empty or consists only of whitespace.

---

## Usage

### Example 1: Checking thread membership and depth before replying

```csharp
Message incoming = messageRepository.GetById(incomingMessageId);

if (!incoming.IsInThread())
{
    // This is a standalone message; start a new thread context.
    logger.Information("Message {Id} is not part of any thread.", incoming.Id);
}

int depth = incoming.GetConversationDepth();
if (depth >= MAX_ALLOWED_DEPTH)
{
    throw new BusinessRuleException("Cannot reply: maximum thread depth exceeded.");
}

Message reply = incoming.CreateReply(replyContent, currentUserId);
messageRepository.Save(reply);
```

### Example 2: Aggregating thread statistics for analytics

```csharp
public ThreadStatistics ComputeThreadStats(Message rootMessage)
{
    int totalMessages = rootMessage.GetConversationCount();
    int maxDepth = 0;

    foreach (var descendant in FlattenReplies(rootMessage))
    {
        int depth = descendant.GetConversationDepth();
        if (depth > maxDepth)
        {
            maxDepth = depth;
        }
    }

    return new ThreadStatistics
    {
        ThreadId = rootMessage.Id,
        TotalMessages = totalMessages,
        MaxDepth = maxDepth,
        StartedAt = rootMessage.CreatedAt
    };
}
```

---

## Notes

- **Circular references:** Both `GetConversationDepth` and `GetConversationCount` include cycle detection. If a message's parent chain loops back to a previously visited message, an `InvalidOperationException` is thrown. This guards against data corruption but does not repair it — the underlying data store must be corrected separately.
- **Thread safety:** These methods are static extension methods that operate on the provided `Message` instance and its in-memory object graph. They do not perform any locking or synchronization. If multiple threads mutate the same `Message` object or its parent/child references concurrently (e.g., simultaneous calls to `CreateReply` on the same parent), the behavior is undefined and may result in lost replies or inconsistent counts. Callers must serialize access to shared message graphs externally.
- **Performance:** `GetConversationCount` walks the entire ancestor and descendant tree, which may be expensive for deeply nested or wide threads with thousands of messages. Consider caching the result or imposing a depth/breadth limit in high-throughput scenarios.
- **`CreateReply` side effects:** This method mutates the parent message's reply collection in addition to returning the new child. The caller receives the new `Message` already wired into the thread; no additional bookkeeping is required to maintain the bidirectional relationship.
- **Null and empty guards:** All methods validate their arguments eagerly. `CreateReply` additionally rejects whitespace-only strings for `content` and `authorId` to prevent semantically empty data from entering the system.
