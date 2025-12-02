import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MyCreatedMissions } from './my-created-missions';

describe('MyCreatedMissions', () => {
  let component: MyCreatedMissions;
  let fixture: ComponentFixture<MyCreatedMissions>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MyCreatedMissions]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MyCreatedMissions);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
