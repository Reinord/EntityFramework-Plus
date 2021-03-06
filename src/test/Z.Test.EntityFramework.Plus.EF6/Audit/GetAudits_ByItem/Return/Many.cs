﻿// Description: Entity Framework Bulk Operations & Utilities (EF Bulk SaveChanges, Insert, Update, Delete, Merge | LINQ Query Cache, Deferred, Filter, IncludeFilter, IncludeOptimize | Audit)
// Website & Documentation: https://github.com/zzzprojects/Entity-Framework-Plus
// Forum & Issues: https://github.com/zzzprojects/EntityFramework-Plus/issues
// License: https://github.com/zzzprojects/EntityFramework-Plus/blob/master/LICENSE
// More projects: http://www.zzzprojects.com/
// Copyright © ZZZ Projects Inc. 2014 - 2016. All rights reserved.

#if EF5 || EF6
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Z.EntityFramework.Plus;
#if EF5 || EF6

#elif EFCORE
using Microsoft.Data.Entity;

#endif

namespace Z.Test.EntityFramework.Plus
{
    public partial class Audit_GetAudits_ByItem
    {
        [TestMethod]
        public void Return_Many()
        {
            TestContext.DeleteAll(x => x.AuditEntryProperties);
            TestContext.DeleteAll(x => x.AuditEntries);
            TestContext.DeleteAll(x => x.Entity_Basics);

            TestContext.Insert(x => x.Entity_Basics, 3);

            var audit = AuditHelper.AutoSaveAudit();
            var audit2 = AuditHelper.AutoSaveAudit();

            Entity_Basic auditedItem = null;
            int originalValue;

            using (var ctx = new TestContext())
            {
                auditedItem = TestContext.Insert(ctx, x => x.Entity_Basics, 3)[2];
                ctx.SaveChanges(audit);

                originalValue = auditedItem.ColumnInt;
                auditedItem.ColumnInt += 2;

                ctx.SaveChanges(audit2);
            }

            // UnitTest - Audit
            {
                using (var ctx = new TestContext())
                {
                    var auditEntries = ctx.AuditEntries.Where(auditedItem).ToList();

                    Assert.AreEqual(2, auditEntries.Count);

                    {
                        var auditEntry = auditEntries[0];
                        Assert.AreEqual(auditedItem.ID.ToString(), auditEntry.Properties.Single(x => x.PropertyName == "ID").NewValueFormatted);
                        Assert.AreEqual(originalValue.ToString(), auditEntry.Properties.Single(x => x.PropertyName == "ColumnInt").NewValueFormatted);
                        Assert.AreEqual("2", auditEntry.Properties.Single(x => x.PropertyName == "ColumnInt").NewValueFormatted);
                    }

                    {
                        var auditEntry = auditEntries[1];
                        Assert.AreEqual(auditedItem.ID.ToString(), auditEntry.Properties.Single(x => x.PropertyName == "ID").NewValueFormatted);
                        Assert.AreEqual(originalValue.ToString(), auditEntry.Properties.Single(x => x.PropertyName == "ColumnInt").OldValueFormatted);
                        Assert.AreEqual(auditedItem.ColumnInt.ToString(), auditEntry.Properties.Single(x => x.PropertyName == "ColumnInt").NewValueFormatted);
                        Assert.AreEqual("2", auditEntry.Properties.Single(x => x.PropertyName == "ColumnInt").OldValueFormatted);
                        Assert.AreEqual("4", auditEntry.Properties.Single(x => x.PropertyName == "ColumnInt").NewValueFormatted);

                    }
                }
            }
        }
    }
}

#endif