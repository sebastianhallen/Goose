namespace Goose.Core.Output
{    
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using Goose.Core.Action;    

    public class GooseErrorTaskHandler
        : IErrorTaskHandler
    {
        private readonly IErrorListProviderFacade errorListFacade;
        private readonly ConcurrentDictionary<ShellCommand, IEnumerable<IGooseErrorTask>> knownErrors;

        public GooseErrorTaskHandler(IErrorListProviderFacade errorListFacade)
        {
            this.errorListFacade = errorListFacade;
            this.knownErrors = new ConcurrentDictionary<ShellCommand, IEnumerable<IGooseErrorTask>>();
        }

        public void Add(IEnumerable<IGooseErrorTask> tasks)
        {
            var commandGroups = from task in tasks
                                group task by task.Command into tasksByCommand
                                select tasksByCommand;
            foreach (var taskGroup in commandGroups)
            {
                this.knownErrors.AddOrUpdate(taskGroup.Key, key =>
                    {
                        var tasksToAdd = taskGroup.ToArray();
                        foreach (var errorTask in tasksToAdd)
                        {
                            this.errorListFacade.Add(errorTask);
                        }
                        return tasksToAdd;
                    }, (key, existing) =>
                        {
                            var tasksToAdd = taskGroup.ToArray();
                            foreach (var errorTask in tasksToAdd)
                            {
                                this.errorListFacade.Add(errorTask);
                            }
                            return existing.Concat(tasksToAdd);
                        });
            }            
        }

        public void Remove(ShellCommand command)
        {
            IEnumerable<IGooseErrorTask> errors;
            if (this.knownErrors.TryRemove(command, out errors))
            {
                foreach (var error in errors)
                {
                    this.errorListFacade.Remove(error);
                }
            }
        }

        public IEnumerable<IGooseErrorTask> Existing(ShellCommand command)
        {
            return this.knownErrors.GetOrAdd(command, key => Enumerable.Empty<IGooseErrorTask>());            
        }
    }
}