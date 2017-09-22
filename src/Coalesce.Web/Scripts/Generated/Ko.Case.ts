
/// <reference path="../coalesce.dependencies.d.ts" />

// Knockout View Model for: Case
// Auto Generated by IntelliTect.Coalesce

module ViewModels {

	export class Case extends Coalesce.BaseViewModel<Case>
    {
        protected modelName = "Case";
        protected primaryKeyName = "caseKey";
        protected modelDisplayName = "Case";

        protected apiController = "/Case";
        protected viewController = "/Case";
    
        /** 
            The enumeration of all possible values of this.dataSource.
        */
        public dataSources: typeof ListViewModels.CaseDataSources = ListViewModels.CaseDataSources;

        /**
            The data source on the server to use when retrieving the object.
            Valid values are in this.dataSources.
        */
        public dataSource: ListViewModels.CaseDataSources = ListViewModels.CaseDataSources.Default;

        /** Behavioral configuration for all instances of Case. Can be overidden on each instance via instance.coalesceConfig. */
        public static coalesceConfig: Coalesce.ViewModelConfiguration<Case>
            = new Coalesce.ViewModelConfiguration<Case>(Coalesce.GlobalConfiguration.viewModel);

        /** Behavioral configuration for the current Case instance. */
        public coalesceConfig: Coalesce.ViewModelConfiguration<Case>
            = new Coalesce.ViewModelConfiguration<Case>(Case.coalesceConfig);
    

        /** The Primary key for the Case object */
        public caseKey: KnockoutObservable<number> = ko.observable(null);
        public title: KnockoutObservable<string> = ko.observable(null);
        public description: KnockoutObservable<string> = ko.observable(null);
        public openedAt: KnockoutObservable<moment.Moment> = ko.observable(moment());
        public assignedToId: KnockoutObservable<number> = ko.observable(null);
        public assignedTo: KnockoutObservable<ViewModels.Person> = ko.observable(null);
        public reportedById: KnockoutObservable<number> = ko.observable(null);
        public reportedBy: KnockoutObservable<ViewModels.Person> = ko.observable(null);
        public attachment: KnockoutObservable<string> = ko.observable(null);
        public severity: KnockoutObservable<string> = ko.observable(null);
        public status: KnockoutObservable<number> = ko.observable(null);
        /** Text value for enumeration Status */
        public statusText: KnockoutComputed<string> = ko.pureComputed(() => {
            for(var i = 0; i < this.statusValues.length; i++){
                if (this.statusValues[i].id == this.status()){
                    return this.statusValues[i].value;
                }
            }
        });
        public caseProducts: KnockoutObservableArray<ViewModels.CaseProduct> = ko.observableArray([]);
        public products: KnockoutObservableArray<ViewModels.Product> = ko.observableArray([]);
        public devTeamAssignedId: KnockoutObservable<number> = ko.observable(null);
        public devTeamAssigned: KnockoutObservable<ViewModels.DevTeam> = ko.observable(null);

       
        /** Display text for AssignedTo */
        public assignedToText: KnockoutComputed<string>;
        /** Display text for ReportedBy */
        public reportedByText: KnockoutComputed<string>;
        /** Display text for DevTeamAssigned */
        public devTeamAssignedText: KnockoutComputed<string>;
        

        /** Url for a table view of all members of collection CaseProducts for the current object. */
        public caseProductsListUrl: KnockoutComputed<string> = ko.computed({
            read: () => {
                     return this.coalesceConfig.baseViewUrl() + '/CaseProduct/Table?caseId=' + this.caseKey();
            },
            deferEvaluation: true
        });

        /** Pops up a stock editor for object assignedTo */
        public showAssignedToEditor: (callback?: any) => void;
        /** Pops up a stock editor for object reportedBy */
        public showReportedByEditor: (callback?: any) => void;
        /** Pops up a stock editor for object devTeamAssigned */
        public showDevTeamAssignedEditor: (callback?: any) => void;


        /** Array of all possible names & values of enum status */
        public statusValues: EnumValue[] = [ 
            { id: 0, value: 'Open' },
            { id: 1, value: 'In Progress' },
            { id: 2, value: 'Resolved' },
            { id: 3, value: 'Closed No Solution' },
            { id: 4, value: 'Cancelled' },
        ];


        /** 
            Load the ViewModel object from the DTO. 
            @param force: Will override the check against isLoading that is done to prevent recursion. False is default.
            @param allowCollectionDeletes: Set true when entire collections are loaded. True is the default. In some cases only a partial collection is returned, set to false to only add/update collections.
        */
        public loadFromDto = (data: any, force: boolean = false, allowCollectionDeletes: boolean = true) => {
            if (!data || (!force && this.isLoading())) return;
            this.isLoading(true);
            // Set the ID 
            this.myId = data.caseKey;
            this.caseKey(data.caseKey);
            // Load the lists of other objects
            if (data.caseProducts != null) {
                // Merge the incoming array
                Coalesce.KnockoutUtilities.RebuildArray(this.caseProducts, data.caseProducts, 'caseProductId', CaseProduct, this, allowCollectionDeletes);
                // Add many-to-many collection
                var objs = [];
                $.each(data.caseProducts, (index, item) => {
                    if (item.product){
                        objs.push(item.product);
                    }
                });
                Coalesce.KnockoutUtilities.RebuildArray(this.products, objs, 'productId', Product, this, allowCollectionDeletes);
            } 
            // Objects are loaded first so that they are available when the IDs get loaded.
            // This handles the issue with populating select lists with correct data because we now have the object.
            if (!data.assignedTo) { 
                if (data.assignedToId != this.assignedToId()) {
                    this.assignedTo(null);
                }
            }else {
                if (!this.assignedTo()){
                    this.assignedTo(new Person(data.assignedTo, this));
                }else{
                    this.assignedTo().loadFromDto(data.assignedTo);
                }
                if (this.parent && this.parent.myId == this.assignedTo().myId && Coalesce.Utilities.getClassName(this.parent) == Coalesce.Utilities.getClassName(this.assignedTo()))
                {
                    this.parent.loadFromDto(data.assignedTo, undefined, false);
                }
            }
            if (!data.reportedBy) { 
                if (data.reportedById != this.reportedById()) {
                    this.reportedBy(null);
                }
            }else {
                if (!this.reportedBy()){
                    this.reportedBy(new Person(data.reportedBy, this));
                }else{
                    this.reportedBy().loadFromDto(data.reportedBy);
                }
                if (this.parent && this.parent.myId == this.reportedBy().myId && Coalesce.Utilities.getClassName(this.parent) == Coalesce.Utilities.getClassName(this.reportedBy()))
                {
                    this.parent.loadFromDto(data.reportedBy, undefined, false);
                }
            }
            if (!data.devTeamAssigned) { 
                if (data.devTeamAssignedId != this.devTeamAssignedId()) {
                    this.devTeamAssigned(null);
                }
            }else {
                if (!this.devTeamAssigned()){
                    this.devTeamAssigned(new DevTeam(data.devTeamAssigned, this));
                }else{
                    this.devTeamAssigned().loadFromDto(data.devTeamAssigned);
                }
                if (this.parent && this.parent.myId == this.devTeamAssigned().myId && Coalesce.Utilities.getClassName(this.parent) == Coalesce.Utilities.getClassName(this.devTeamAssigned()))
                {
                    this.parent.loadFromDto(data.devTeamAssigned, undefined, false);
                }
            }

            // The rest of the objects are loaded now.
            this.title(data.title);
            this.description(data.description);
            if (data.openedAt == null) this.openedAt(null);
            else if (this.openedAt() == null || !this.openedAt().isSame(moment(data.openedAt))){
                this.openedAt(moment(data.openedAt));
            }
            this.assignedToId(data.assignedToId);
            this.reportedById(data.reportedById);
            this.attachment(data.attachment);
            this.severity(data.severity);
            this.status(data.status);
            this.devTeamAssignedId(data.devTeamAssignedId);
            if (this.coalesceConfig.onLoadFromDto()){
                this.coalesceConfig.onLoadFromDto()(this as any);
            }
            this.isLoading(false);
            this.isDirty(false);
            this.validate();
        };

        /** Save the object into a DTO */
        public saveToDto = (): any => {
            var dto: any = {};
            dto.caseKey = this.caseKey();

            dto.title = this.title();
            dto.description = this.description();
            if (!this.openedAt()) dto.OpenedAt = null;
            else dto.openedAt = this.openedAt().format('YYYY-MM-DDTHH:mm:ssZZ');
            dto.assignedToId = this.assignedToId();
            if (!dto.assignedToId && this.assignedTo()) {
                dto.assignedToId = this.assignedTo().personId();
            }
            dto.reportedById = this.reportedById();
            if (!dto.reportedById && this.reportedBy()) {
                dto.reportedById = this.reportedBy().personId();
            }
            dto.attachment = this.attachment();
            dto.severity = this.severity();
            dto.status = this.status();
            dto.devTeamAssignedId = this.devTeamAssignedId();
            if (!dto.devTeamAssignedId && this.devTeamAssigned()) {
                dto.devTeamAssignedId = this.devTeamAssigned().devTeamId();
            }

            return dto;
        }


        constructor(newItem?: any, parent?: any){
            super();
            var self = this;
            self.parent = parent;
            self.myId;
            
            ko.validation.init({
                grouping: {
                    deep: true,
                    live: true,
                    observable: true
                }
            });

            // SetupValidation {
			self.title = self.title.extend({ required: {params: true, message: "You must enter a title for the case."} });
			self.openedAt = self.openedAt.extend({ moment: { unix: true },  });
            
            self.errors = ko.validation.group([
                self.caseKey,
                self.title,
                self.description,
                self.openedAt,
                self.assignedToId,
                self.assignedTo,
                self.reportedById,
                self.reportedBy,
                self.attachment,
                self.severity,
                self.status,
                self.caseProducts,
                self.devTeamAssignedId,
                self.devTeamAssigned,
            ]);
            self.warnings = ko.validation.group([
            ]);

            // Computed Observable for edit URL
            self.editUrl = ko.computed(function() {
                return self.coalesceConfig.baseViewUrl() + self.viewController + "/CreateEdit?id=" + self.caseKey();
            });

            // Create computeds for display for objects
			self.assignedToText = ko.computed(function()
			{   // If the object exists, use the text value. Otherwise show 'None'
				if (self.assignedTo() && self.assignedTo().name()) {
					return self.assignedTo().name().toString();
				} else {
					return "None";
				}
			});
			self.reportedByText = ko.computed(function()
			{   // If the object exists, use the text value. Otherwise show 'None'
				if (self.reportedBy() && self.reportedBy().name()) {
					return self.reportedBy().name().toString();
				} else {
					return "None";
				}
			});
			self.devTeamAssignedText = ko.computed(function()
			{   // If the object exists, use the text value. Otherwise show 'None'
				if (self.devTeamAssigned() && self.devTeamAssigned().name()) {
					return self.devTeamAssigned().name().toString();
				} else {
					return "None";
				}
			});

    


            self.showAssignedToEditor = function(callback: any) {
                if (!self.assignedTo()) {
                    self.assignedTo(new Person());
                }
                self.assignedTo().showEditor(callback)
            };
            self.showReportedByEditor = function(callback: any) {
                if (!self.reportedBy()) {
                    self.reportedBy(new Person());
                }
                self.reportedBy().showEditor(callback)
            };

            // Load all child objects that are not loaded.
            self.loadChildren = function(callback) {
                var loadingCount = 0;
                // See if self.assignedTo needs to be loaded.
                if (self.assignedTo() == null && self.assignedToId() != null){
                    loadingCount++;
                    var assignedToObj = new Person();
                    assignedToObj.load(self.assignedToId(), function() {
                        loadingCount--;
                        self.assignedTo(assignedToObj);
                        if (loadingCount == 0 && $.isFunction(callback)){
                            callback();
                        }
                    });
                }
                // See if self.reportedBy needs to be loaded.
                if (self.reportedBy() == null && self.reportedById() != null){
                    loadingCount++;
                    var reportedByObj = new Person();
                    reportedByObj.load(self.reportedById(), function() {
                        loadingCount--;
                        self.reportedBy(reportedByObj);
                        if (loadingCount == 0 && $.isFunction(callback)){
                            callback();
                        }
                    });
                }
                // See if self.devTeamAssigned needs to be loaded.
                if (self.devTeamAssigned() == null && self.devTeamAssignedId() != null){
                    loadingCount++;
                    var devTeamAssignedObj = new DevTeam();
                    devTeamAssignedObj.load(self.devTeamAssignedId(), function() {
                        loadingCount--;
                        self.devTeamAssigned(devTeamAssignedObj);
                        if (loadingCount == 0 && $.isFunction(callback)){
                            callback();
                        }
                    });
                }
                if (loadingCount == 0 && $.isFunction(callback)){
                    callback();
                }
            };

            // This stuff needs to be done after everything else is set up.
            // Complex Type Observables

            self.title.subscribe(self.autoSave);
            self.description.subscribe(self.autoSave);
            self.openedAt.subscribe(self.autoSave);
            self.assignedToId.subscribe(self.autoSave);
            self.assignedTo.subscribe(self.autoSave);
            self.reportedById.subscribe(self.autoSave);
            self.reportedBy.subscribe(self.autoSave);
            self.attachment.subscribe(self.autoSave);
            self.severity.subscribe(self.autoSave);
            self.status.subscribe(self.autoSave);
            self.devTeamAssignedId.subscribe(self.autoSave);
            self.devTeamAssigned.subscribe(self.autoSave);
        
            self.products.subscribe(function(changes){
                if (!self.isLoading() && changes.length > 0){
                    for (var i in changes){
                        var change:any = changes[i];
                        self.autoSaveCollection('products', change.value.productId(), change.status);
                    }
                }
            }, null, "arrayChange");
            
            if (newItem) {
                if ($.isNumeric(newItem)) self.load(newItem);
                else self.loadFromDto(newItem, true);
            }
        }
    }





    export namespace Case {
        export enum StatusEnum {
            Open = 0,
            InProgress = 1,
            Resolved = 2,
            ClosedNoSolution = 3,
            Cancelled = 4,
        };

        // Classes for use in method calls to support data binding for input for arguments
    }
}