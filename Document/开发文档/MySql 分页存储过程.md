
### 这是一个 Mysql 分页存储过程

```sql
CREATE PROCEDURE sp_Pagination(
    IN TableName VARCHAR(255),         -- 数据表名。
    IN SelectField VARCHAR(255),       -- 选择字段。
    IN SortString VARCHAR(255),        -- 排序字符串。
    IN ArrangeField VARCHAR(255),      -- 排列字段。
    IN PageSize INT,                   -- 每页行数。
    IN ThisPage INT,                   -- 当前页码。
    IN SearchType BIT,                 -- 查询类型（0 返回数据，1 返回总行数）。
    IN SearchCondition VARCHAR(1500)   -- 查询条件。
)
BEGIN
    DECLARE SQLString TEXT;            -- SQL 查询字符串。
    DECLARE OffsetValue INT;           -- 偏移值。

    -- 设置默认值。
    IF SelectField IS NULL THEN
        SET SelectField = '*';
    END IF;

    IF SortString IS NULL THEN
        SET SortString = '';
    END IF;

    IF ArrangeField IS NULL THEN
        SET ArrangeField = '';
    END IF;

    IF PageSize IS NULL THEN
        SET PageSize = 20;
    END IF;

    IF ThisPage IS NULL THEN
        SET ThisPage = 1;
    END IF;

    IF SearchType IS NULL THEN
        SET SearchType = 1;
    END IF;

    IF SearchCondition IS NULL THEN
        SET SearchCondition = '';
    END IF;

    -- 计算偏移值。
    SET OffsetValue = (ThisPage - 1) * PageSize;

    -- 判断查询类型。
    IF SearchType != 0 THEN
        -- 查询总行数。
        SET SQLString = CONCAT('SELECT COUNT(*) AS AllCount FROM `', TableName, '` WHERE 1 = 1 ', ' ', SearchCondition);
    ELSE
        -- 查询具体数据。
        IF ThisPage = 1 THEN
            -- 查询第一页的数据。
            SET SQLString = CONCAT('SELECT ', SelectField, ', @rownum := @rownum + 1 AS CB ',
                                   'FROM (SELECT @rownum := 0) r, `', TableName, '` t WHERE 1 = 1 ', ' ', SearchCondition, ' ',
                                   SortString, ' LIMIT ', PageSize);
        ELSE
            -- 查询非第一页的数据。
            SET SQLString = CONCAT(
                'SELECT t.', SelectField, ', @rownum := @rownum + 1 + ', OffsetValue, ' AS CB ',
                'FROM (SELECT @rownum := 0) r, `', TableName, '` t ',
                'INNER JOIN (SELECT `', ArrangeField, '` FROM `', TableName, '` WHERE 1 = 1 ', ' ', SearchCondition, ' ',
                SortString, ' LIMIT ', OffsetValue, ', ', PageSize, ') AS tmp ON t.`', ArrangeField, '` = tmp.`', ArrangeField, '` ',
                'WHERE 1 = 1 ', ' ', SearchCondition, ' ', SortString, ' LIMIT ', PageSize
            );
        END IF;
    END IF;

    -- 预处理和执行 SQL 语句。
    SET @SQLString = SQLString;
    PREPARE stmt FROM @SQLString;
    EXECUTE stmt;
    DEALLOCATE PREPARE stmt;
END;
```
