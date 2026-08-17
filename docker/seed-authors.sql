INSERT INTO "Authors" ("Id", "Name")
SELECT seed."Id", seed."Name"
FROM (
    VALUES
        ('3f6c1a52-8d4b-4f0e-9c17-2b5a7e1d9c04'::uuid, 'Aristoteles'),
        ('7a9e2d18-64c3-4b7a-8e51-0d3f6b8c2a15'::uuid, 'William Shakespeare'),
        ('c5d81b73-2e46-49af-bd0c-91a7f4e63d28'::uuid, 'Franz Kafka')
) AS seed ("Id", "Name")
WHERE NOT EXISTS (SELECT 1 FROM "Authors");
