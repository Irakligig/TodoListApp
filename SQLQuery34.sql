-- Check all todo lists and owners
SELECT tl.Id, tl.Name, tl.OwnerId, u.Id AS UserId, u.UserName
FROM TodoLists tl
LEFT JOIN [User] u ON tl.OwnerId = u.Id;
