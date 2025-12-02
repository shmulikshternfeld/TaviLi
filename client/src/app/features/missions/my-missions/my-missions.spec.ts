import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MyMissions } from './my-missions';

describe('MyMissions', () => {
  let component: MyMissions;
  let fixture: ComponentFixture<MyMissions>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MyMissions]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MyMissions);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
